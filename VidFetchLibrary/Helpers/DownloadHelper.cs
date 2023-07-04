using Microsoft.Extensions.Caching.Memory;
using VidFetchLibrary.Library;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using VidFetchLibrary.Data;
using VidFetchLibrary.DataAccess;

namespace VidFetchLibrary.Helpers;
public class DownloadHelper : IDownloadHelper
{
    private const int CacheTime = 5;
    private readonly IPathHelper _pathHelper;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _cachingHelper;
    private readonly ISettingsData _settingsData;
    private readonly YoutubeClient _youtube;

    public DownloadHelper(IPathHelper pathHelper,
                          IMemoryCache cache,
                          ICachingHelper cachingHelper,
                          ISettingsData settingsData,
                          YoutubeClient youtube)
    {
        _pathHelper = pathHelper;
        _cache = cache;
        _cachingHelper = cachingHelper;
        _settingsData = settingsData;
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

            var settings = await _settingsData.GetSettingsAsync();

            string path = settings.FfmpegPath;
            if (File.Exists(path) && string.IsNullOrWhiteSpace(path) is false)
            {
                await DownloadWithFfmpeg(
                    video,
                    settings,
                    isPlaylist,
                    playlistTitle,
                    progress,
                    token);
            }
            else
            {
                await DownloadWithoutFfmpeg(
                    video,
                    settings,
                    videoUrl,
                    isPlaylist,
                    playlistTitle,
                    progress,
                    token);
            }

            if (settings.DownloadSubtitles)
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
        SettingsLibrary settings,
        bool isPlaylist,
        string playlistTitle,
        IProgress<double> progress, 
        CancellationToken token)
    {
        var streamInfos = await LoadStreamInfosAsync(_youtube, video, settings, token);
        var conversionRequest = await GetRequestBuilder(settings, video, isPlaylist, playlistTitle);

        await _youtube.Videos.DownloadAsync(streamInfos, conversionRequest, progress, token);
    }

    private async Task DownloadWithoutFfmpeg(
        Video video,
        SettingsLibrary settings,
        string videoUrl,
        bool isPlaylist,
        string playlistTitle,
        IProgress<double> progress,
        CancellationToken token)
    {
        var streamManifest = await _youtube.Videos.Streams
                    .GetManifestAsync(videoUrl, token);
        var streamInfo = GetVideoStream(streamManifest, settings);

        string downloadFolder = await _pathHelper.GetVideoDownloadPath(
            settings,
            video.Title,
            isPlaylist,
            playlistTitle);

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

        string videoFolder = await _pathHelper.OpenFolderLocation();
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
        SettingsLibrary settings,
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

        videoStreamInfo = GetFfmpegVideoStream(streamManifest, settings);

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

    private async Task<ConversionRequest> GetRequestBuilder(
        SettingsLibrary settings,
        Video video,
        bool isPlaylist,
        string playlistTitle)
    {
        string downloadFolder = await _pathHelper.GetVideoDownloadPath(settings, video.Title, isPlaylist, playlistTitle);

        var requestBuilder = new ConversionRequestBuilder(downloadFolder)
            .SetFFmpegPath(settings.FfmpegPath)
            .SetPreset(ConversionPreset.UltraFast)
            .SetContainer(Container.Mp4)
            .Build();

        return requestBuilder;
    }

    private IVideoStreamInfo GetFfmpegVideoStream(
        StreamManifest streamManifest,
        SettingsLibrary settings)
    {
        var highestVideoResolutionStream = streamManifest.GetVideoStreams()
                    .GetWithHighestVideoQuality();
        try
        {
            var videoStreamInfo = settings.SelectedResolution switch
            {
                VideoResolution.HighestResolution => highestVideoResolutionStream,

                _ => streamManifest.GetVideoStreams()
                    .First(s => s.VideoQuality.Label == GetVideoQualityLabel(settings)),
            };

            return videoStreamInfo;
        }
        catch
        {
            return highestVideoResolutionStream;
        }
    }

    private IVideoStreamInfo GetVideoStream(
        StreamManifest streamManifest,
        SettingsLibrary settings)
    {
        var highestResolutionStream = streamManifest.GetMuxedStreams()
                .GetWithHighestVideoQuality();
        try
        {
            var videoStreamInfo = settings.SelectedResolution switch
            {
                VideoResolution.HighestResolution => highestResolutionStream,

                _ => streamManifest.GetMuxedStreams()
                    .First(s => s.VideoQuality.Label == GetVideoQualityLabel(settings)),
            };

            return videoStreamInfo;
        }
        catch
        {
            return highestResolutionStream;
        }
    }

    private string GetVideoQualityLabel(SettingsLibrary settings)
    {
        VideoResolution resolution = settings.SelectedResolution;
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
