using Microsoft.Extensions.Caching.Memory;
using System.Web;
using VidFetchLibrary.Helpers;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Client;
public class Youtube : IYoutube
{
    private readonly IDownloadHelper _downloaderHelper;
    private readonly IMemoryCache _cache;
    private readonly ICachingHelper _cachingHelper;
    private YoutubeClient _client;

    public Youtube(IDownloadHelper downloaderHelper,
                             IMemoryCache cache,
                             ICachingHelper cachingHelper)
    {
        _downloaderHelper = downloaderHelper;
        _cache = cache;
        _cachingHelper = cachingHelper;
        InitializeClient();
    }

    private void InitializeClient()
    {
        _client = new();
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
            string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");
            var playlistVideos = await _client.Playlists.GetVideosAsync(playlistId);

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
            output = await _client.Videos.GetAsync(url);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<Channel> GetChannelAsync(string url)
    {
        string key = _cachingHelper.CacheChannelKey(url);

        var output = _cache.Get<Channel>(key);
        if (output is null)
        {
            output = await _client.Channels.GetAsync(url);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<List<VideoSearchResult>> GetVideosBySearchAsync(string searchInput)
    {
        var results = await _client.Search.GetVideosAsync(searchInput);
        return results.ToList();
    }

    public async Task<List<ChannelSearchResult>> GetChannelBySearchAsync(string searchInput)
    {
        var results = await _client.Search.GetChannelsAsync(searchInput);
        return results.ToList();
    }

    public async Task<List<PlaylistSearchResult>> GetPlaylistsBySearchAsync(string searchInput)
    {
        var results = await _client.Search.GetPlaylistsAsync(searchInput);
        return results.ToList();
    }

    private static string GetPlaylistId(string url) 
    {
        string queryString = new Uri(url).Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);
        return queryParams.Get("list");
    }
}
