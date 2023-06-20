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
    private const int MaxDataAmount = 20;

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

    public async Task<Playlist> GetPlaylistAsync(string url)
    {
        string key = _cachingHelper.CachePlaylistKey(url);

        var output = _cache.Get<Playlist>(key);
        if (output is null)
        {
            output = await _client.Playlists.GetAsync(url);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<List<VideoSearchResult>> GetVideosBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"VideoSearch-{searchInput}";

        var output = _cache.Get<List<VideoSearchResult>>(key);
        if (output is null)
        {
            var results = await _client.Search.GetVideosAsync(searchInput, token);
            output = results.Take(MaxDataAmount).ToList();
        }

        return output;
    }

    public async Task<List<ChannelSearchResult>> GetChannelBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"ChannelSearch-{searchInput}";

        var output = _cache.Get<List<ChannelSearchResult>>(key);
        if (output is null)
        {
            var results = await _client.Search.GetChannelsAsync(searchInput, token);
            output = results.Take(MaxDataAmount).ToList();
        }

        return output;
    }

    public async Task<List<PlaylistSearchResult>> GetPlaylistsBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"PlaylistSearch-{searchInput}";

        var output = _cache.Get<List<PlaylistSearchResult>>(key);
        if (output is null)
        {
            var results = await _client.Search.GetPlaylistsAsync(searchInput);
            output = results.Take(MaxDataAmount).ToList();
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
