using Microsoft.Extensions.Caching.Memory;
using VidFetchLibrary.Library;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using VidFetchLibrary.Data;

namespace VidFetchLibrary.Helpers;
public class DownloadHelper : IDownloadHelper
{
    private const int CacheTime = 5;
    private readonly IPathHelper _pathHelper;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _cachingHelper;
    private readonly ISettingsLibrary _settings;
    private readonly YoutubeClient _youtube;

    public DownloadHelper(IPathHelper pathHelper,
                          IMemoryCache cache,
                          ICachingHelper cachingHelper,
                          ISettingsLibrary settings, 
                          YoutubeClient youtube)
    {
        _pathHelper = pathHelper;
        _cache = cache;
        _cachingHelper = cachingHelper;
        _settings = settings;
        _youtube = youtube;
    }

    public async Task DownloadVideoAsync(
        string videoUrl,
        IProgress<double> progress,
        CancellationToken token,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        try
        {
            var video = await LoadVideoAsync(videoUrl, token)
                ?? throw new NullReferenceException("Video not found.");

            if (File.Exists(_settings.FfmpegPath))
            {
                await DownloadWithFfmpeg(
                    video,
                    token,
                    isPlaylist,
                    playlistTitle,
                    progress);
            }
            else
            {
                await DownloadWithoutFfmpeg(
                    video,
                    videoUrl,
                    token,
                    isPlaylist,
                    playlistTitle,
                    progress);
            }

            if (_settings.DownloadSubtitles)
            {
                await DownloadSubtitlesAsync(video, token);
            }
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Task has been cancelled.");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private async Task DownloadWithFfmpeg(
        Video video,
        CancellationToken token,
        bool isPlaylist,
        string playlistTitle,
        IProgress<double> progress)
    {
        var streamInfos = await LoadStreamInfosAsync(_youtube, video, token);
        var conversionRequest = GetRequestBuilder(video, isPlaylist, playlistTitle);

        await _youtube.Videos.DownloadAsync(streamInfos, conversionRequest, progress, token);
    }

    private async Task DownloadWithoutFfmpeg(
        Video video,
        string videoUrl,
        CancellationToken token,
        bool isPlaylist,
        string playlistTitle,
        IProgress<double> progress)
    {
        var streamManifest = await _youtube.Videos.Streams
                    .GetManifestAsync(videoUrl, token);
        var streamInfo = GetVideoStream(streamManifest);

        string downloadFolder = _pathHelper.GetVideoDownloadPath(video.Title, isPlaylist, playlistTitle);

        await _youtube.Videos.Streams.DownloadAsync(streamInfo, downloadFolder, progress, token);
    }

    private async Task DownloadSubtitlesAsync(
        Video video,
        CancellationToken token)
    {
        var subtitleinfo = await LoadSubtitleInfoAsync(_youtube, video, token);

        if (subtitleinfo is null || subtitleinfo.Count <= 0)
        {
            return;
        }

        string videoFolder = _pathHelper.OpenFolderLocation();
        string sanitizedVideoTitle = _pathHelper.GetSanizitedFileName(video.Title);
        string videoFolderPath = Path.Combine(videoFolder, sanitizedVideoTitle);

        if (Directory.Exists(videoFolderPath) is false)
        {
            Directory.CreateDirectory(videoFolderPath);
        }

        foreach (var s in subtitleinfo)
        {
            string sanitizedSubtitle = _pathHelper.GetSanizitedFileName($"{s.Language}");
            string subtitleDownloadFolder = Path.Combine(videoFolderPath, $"{sanitizedSubtitle}.vtt");

            await _youtube.Videos.ClosedCaptions.DownloadAsync(s, subtitleDownloadFolder, cancellationToken: token);
        }
    }

    private async Task<Video> LoadVideoAsync(string url, CancellationToken token)
    {
        var video = await _cachingHelper.LoadYoutubeExplodeVideoAsync(url, token);
        return video;
    }

    private async Task<IStreamInfo[]> LoadStreamInfosAsync(
        YoutubeClient client,
        Video video,
        CancellationToken token)
    {
        string key = _cachingHelper.CacheStreamManifest(video.Url);
        IVideoStreamInfo videoStreamInfo;

        var streamManifest = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            return await client.Videos.Streams.GetManifestAsync(video.Id, token);
        });

        if (streamManifest is null)
        {
            _cache.Remove(key);
        }

        var audioStreamInfo = streamManifest
            .GetAudioStreams()
            .Where(s => s.Container == Container.Mp4)
            .GetWithHighestBitrate();

        videoStreamInfo = GetFfmpegVideoStream(streamManifest);

        return new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
    }

    private async Task<List<ClosedCaptionTrackInfo>> LoadSubtitleInfoAsync(
        YoutubeClient client,
        Video video,
        CancellationToken token)
    {
        string key = _cachingHelper.CacheSubtitlesInfoKey(video.Url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var subtitleManifest = await client.Videos.ClosedCaptions.GetManifestAsync(video.Id, token);
            var subtitleinfo = subtitleManifest.Tracks;

            return subtitleinfo.ToList();
        });

        if (output is null)
        {
            _cache.Remove(key);
        }

        return output;
    }

    private ConversionRequest GetRequestBuilder(Video video, bool isPlaylist, string playlistTitle)
    {
        string downloadFolder = _pathHelper.GetVideoDownloadPath(video.Title, isPlaylist, playlistTitle);

        var requestBuilder = new ConversionRequestBuilder(downloadFolder)
            .SetFFmpegPath(_settings.FfmpegPath)
            .SetPreset(ConversionPreset.UltraFast)
            .SetContainer(Container.Mp4)
            .Build();

        return requestBuilder;
    }

    private IVideoStreamInfo GetFfmpegVideoStream(StreamManifest streamManifest)
    {
        var highestVideoResolutionStream = streamManifest.GetVideoStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .GetWithHighestVideoQuality();
        try
        {
            IVideoStreamInfo videoStreamInfo = null;

            videoStreamInfo = _settings.SelectedResolution switch
            {
                VideoResolution.HighestResolution => highestVideoResolutionStream,

                _ => streamManifest.GetVideoStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .First(s => s.VideoQuality.Label == GetVideoQualityLabel()),
            };

            return videoStreamInfo;
        }
        catch
        {
            return highestVideoResolutionStream;
        }
    }

    private IVideoStreamInfo GetVideoStream(StreamManifest streamManifest)
    {
        var highestResolutionStream = streamManifest.GetMuxedStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestVideoQuality();
        try
        {
            IVideoStreamInfo videoStreamInfo = null;

            videoStreamInfo = _settings.SelectedResolution switch
            {
                VideoResolution.HighestResolution => highestResolutionStream,

                _ => streamManifest.GetMuxedStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .First(s => s.VideoQuality.Label == GetVideoQualityLabel()),
            };

            return videoStreamInfo;
        }
        catch
        {
            return highestResolutionStream;
        }
    }

    private string GetVideoQualityLabel()
    {
        VideoResolution resolution = _settings.SelectedResolution;
        string resolutionString = resolution.ToString();

        if (resolutionString.StartsWith("P") && resolutionString.Length > 1)
        {
            resolutionString = resolutionString[1..] + resolutionString[0];
            return resolutionString;
        }
        else
        {
            return resolutionString;
        }
    }
}
