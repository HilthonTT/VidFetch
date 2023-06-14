using Microsoft.Extensions.Caching.Memory;
using System.Web;
using VidFetchLibrary.Helpers;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Downloader;
public class YoutubeDownloader : IYoutubeDownloader
{
    private readonly IDownloadHelper _downloaderHelper;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _cachingHelper;

    public YoutubeDownloader(IDownloadHelper downloaderHelper,
                             IMemoryCache cache,
                             ICachingHelper cachingHelper)
    {
        _downloaderHelper = downloaderHelper;
        _cache = cache;
        _cachingHelper = cachingHelper;
    }

    public async Task DownloadVideoAsync(
        string url,
        string downloadPath,
        string extension,
        IProgress<double> progress,
        CancellationToken token,
        bool downloadSubtitles = false)
    { 
        var youtube = new YoutubeClient();

        await _downloaderHelper.DownloadVideoAsync(
            youtube,
            url,
            downloadPath,
            extension,
            progress,
            token,
            downloadSubtitles);
    }

    public async Task<List<PlaylistVideo>> GetPlayListVideosAsync(string url)
    {
        string key = _cachingHelper.CacheVideoPlaylistKey(url);

        var output = _cache.Get<List<PlaylistVideo>>(key);
        if (output is null)
        {
            var youtube = new YoutubeClient();
            string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");
            var playlistVideos = await youtube.Playlists.GetVideosAsync(playlistId);

            output = playlistVideos.ToList();
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<Video> GetVideoAsync(string url)
    {
        string key = _cachingHelper.CacheVideoKey(url);

        var output = _cache.Get<Video>(key);
        if (output is null)
        {
            var youtube = new YoutubeClient();
            output = await youtube.Videos.GetAsync(url);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    private static string GetPlaylistId(string url) 
    {
        string queryString = new Uri(url).Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);
        return queryParams.Get("list");
    }
}
