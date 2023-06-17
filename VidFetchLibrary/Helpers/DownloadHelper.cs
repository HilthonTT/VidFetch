using Microsoft.Extensions.Caching.Memory;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace VidFetchLibrary.Helpers;
public class DownloadHelper : IDownloadHelper
{
    private const string VideoNotFoundErrorMessage = "Video not found.";
    private const string NoSuitableVideoStreamErrorMessage = "No suitable video stream found";
    private readonly IPathHelper _pathHelper;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _cachingHelper;

    public DownloadHelper(IPathHelper pathHelper,
                          IMemoryCache cache,
                          ICachingHelper cachingHelper)
    {
        _pathHelper = pathHelper;
        _cache = cache;
        _cachingHelper = cachingHelper;
    }

    public async Task DownloadVideoAsync(
        YoutubeClient client,
        string videoUrl,
        string path,
        string extension,
        IProgress<double> progress,
        CancellationToken token,
        bool downloadSubtitles = false)
    {
        try
        {
            var video = await LoadVideoAsync(client, videoUrl, token) ?? throw new Exception(VideoNotFoundErrorMessage);
            var streamInfo = await LoadStreamInfoAsync(client, video, token);

            string sanitizedTitle = GetSanizitedFileName(video.Title);
            string downloadFolder = _pathHelper.GetVideoDownloadPath(sanitizedTitle, extension, path);

            await client.Videos.Streams.DownloadAsync(streamInfo, downloadFolder, progress, token);

            if (downloadSubtitles)
            {
                await DownloadSubtitlesAsync(client, video, path, token);
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
        string path, 
        CancellationToken token)
    {
        var subtitleinfo = await LoadSubtitleInfoAsync(client, video, token);

        if (subtitleinfo is null || subtitleinfo.Count <= 0)
        {
            return;
        }

        string videoFolder = _pathHelper.GetVideoDownloadPath("", "", path);
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

        var output = _cache.Get<Video>(key);
        if (output is null)
        {
            output = await client.Videos.GetAsync(url, token);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    private async Task<IStreamInfo> LoadStreamInfoAsync(
        YoutubeClient client,
        Video video,
        CancellationToken token)
    {
        string key = _cachingHelper.CacheStreamInfoKey(video.Url);

        var output = _cache.Get<IStreamInfo>(key);
        if (output is null)
        {
            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id, token);
            output = streamManifest
                .GetMuxedStreams()
                .TryGetWithHighestVideoQuality()
                ?? throw new Exception(NoSuitableVideoStreamErrorMessage);    

            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    private async Task<List<ClosedCaptionTrackInfo>> LoadSubtitleInfoAsync(
        YoutubeClient client,
        Video video,
        CancellationToken token)
    {
        string key = _cachingHelper.CacheSubtitlesInfoKey(video.Url);

        var output = _cache.Get<List<ClosedCaptionTrackInfo>>(key);
        if (output is null)
        {
            var subtitleManifest = await client.Videos.ClosedCaptions.GetManifestAsync(video.Id, token);
            var subtitleinfo = subtitleManifest.Tracks;
            output = subtitleinfo.ToList();

            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    private static string GetSanizitedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }
}
