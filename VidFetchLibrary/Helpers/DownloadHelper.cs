using Microsoft.Extensions.Caching.Memory;
using VidFetchLibrary.Library;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;

namespace VidFetchLibrary.Helpers;
public class DownloadHelper : IDownloadHelper
{
    private const string VideoNotFoundErrorMessage = "Video not found.";
    private const int CacheTime = 5;
    private readonly IPathHelper _pathHelper;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _cachingHelper;
    private readonly ISettingsLibrary _settings;

    public DownloadHelper(IPathHelper pathHelper,
                          IMemoryCache cache,
                          ICachingHelper cachingHelper, 
                          ISettingsLibrary settings)
    {
        _pathHelper = pathHelper;
        _cache = cache;
        _cachingHelper = cachingHelper;
        _settings = settings;
    }

    public async Task DownloadVideoAsync(
        YoutubeClient client,
        string videoUrl,
        IProgress<double> progress,
        CancellationToken token)
    {
        try
        {
            var video = await LoadVideoAsync(client, videoUrl, token) ?? throw new Exception(VideoNotFoundErrorMessage);
            var streamInfos = await LoadStreamInfosAsync(client, video, token);
            var conversionRequest = GetRequestBuilder(video);

            if (File.Exists(_settings.FfmpegPath))
            {
                await client.Videos.DownloadAsync(streamInfos, conversionRequest, progress, token);
            }
            else
            {
                var streamManifest = await client.Videos.Streams
                    .GetManifestAsync(videoUrl, token);

                var streamInfo = streamManifest
                    .GetMuxedStreams()
                    .GetWithHighestVideoQuality();

                string sanitizedTitle = GetSanizitedFileName(video.Title);
                string downloadFolder = _pathHelper.GetVideoDownloadPath(sanitizedTitle);

                await client.Videos.Streams.DownloadAsync(streamInfo, downloadFolder, progress, token);
            }

            if (_settings.DownloadSubtitles)
            {
                await DownloadSubtitlesAsync(client, video, token);
            }
        }
        catch (TaskCanceledException)
        {
            // Do Nothing as it has been purposely ended.
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private async Task DownloadSubtitlesAsync(
        YoutubeClient client,
        Video video,
        CancellationToken token)
    {
        var subtitleinfo = await LoadSubtitleInfoAsync(client, video, token);

        if (subtitleinfo is null || subtitleinfo.Count <= 0)
        {
            return;
        }

        string videoFolder = _pathHelper.OpenFolderLocation();
        string sanitizedVideoTitle = GetSanizitedFileName(video.Title);
        string videoFolderPath = Path.Combine(videoFolder, sanitizedVideoTitle);

        if (Directory.Exists(videoFolderPath) is false)
        {
            Directory.CreateDirectory(videoFolderPath);
        }

        foreach (var s in subtitleinfo)
        {
            string sanitizedSubtitle = GetSanizitedFileName($"{s.Language}");
            string subtitleDownloadFolder = Path.Combine(videoFolderPath, $"{sanitizedSubtitle}.vtt");
            await client.Videos.ClosedCaptions.DownloadAsync(s, subtitleDownloadFolder, cancellationToken: token);
        }
    }

    private async Task<Video> LoadVideoAsync(
        YoutubeClient client,
        string url,
        CancellationToken token)
    {
        string key = _cachingHelper.CacheVideoKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            return await client.Videos.GetAsync(url, token);
        });

        return output;
    }

    private async Task<IStreamInfo[]> LoadStreamInfosAsync(
        YoutubeClient client,
        Video video,
        CancellationToken token)
    {
        string key = _cachingHelper.CacheStreamInfoKey(video.Url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id, token);
            IVideoStreamInfo videoStreamInfo;

            var audioStreamInfo = streamManifest
                .GetAudioStreams()
                .Where(s => s.Container == GetContainer())
                .GetWithHighestBitrate();

            videoStreamInfo = GetVideoStream(streamManifest);

            return new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
        });

        return output;
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

        return output;
    }

    private static string GetSanizitedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }
         
    private ConversionRequest GetRequestBuilder(Video video)
    {
        string sanitizedTitle = GetSanizitedFileName(video.Title);
        string downloadFolder = _pathHelper.GetVideoDownloadPath(sanitizedTitle);

        var requestBuilder = new ConversionRequestBuilder(downloadFolder)
            .SetFFmpegPath(_settings.FfmpegPath)
            .SetPreset(ConversionPreset.UltraFast)
            .Build();

        return requestBuilder;
    }

    private IVideoStreamInfo GetVideoStream(StreamManifest streamManifest)
    {
        try
        {
            IVideoStreamInfo videoStreamInfo = null;

            videoStreamInfo = _settings.SelectedResolution switch
            {
                "Highest Resolution" => streamManifest.GetVideoStreams()
                    .Where(s => s.Container == GetContainer())
                    .GetWithHighestVideoQuality(),

                _ => streamManifest.GetVideoStreams()
                    .Where(s => s.Container == GetContainer())
                    .First(s => s.VideoQuality.Label == _settings.SelectedResolution),
            };

            return videoStreamInfo;
        }
        catch
        {
            return streamManifest.GetVideoStreams()
                .Where(s => s.Container == GetContainer())
                .GetWithHighestVideoQuality();
        }
    }

    private Container GetContainer()
    {
        return _settings.SelectedFormat switch
        {
            ".mp4" => Container.Mp4,
            ".mp3" => Container.Mp3,
            ".tgpp" => Container.Tgpp,
            ".webm" => Container.WebM,
            _ => Container.Mp4,
        };
    } 
}
