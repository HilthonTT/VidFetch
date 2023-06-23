using Microsoft.Extensions.Caching.Memory;
using System.Web;
using VidFetchLibrary.Helpers;
using VidFetchLibrary.Models;
using YoutubeExplode;
using YoutubeExplode.Common;

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

    public async Task<List<VideoModel>> GetPlayListVideosAsync(string url)
    {
        string key = _cachingHelper.CacheVideoList(url);

        var output = _cache.Get<List<VideoModel>>(key);
        if (output is null)
        {
            string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");
            var playlistVideos = await _client.Playlists.GetVideosAsync(playlistId);

            output = playlistVideos
                .Select(v => new VideoModel(v))
                .ToList();

            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<List<VideoModel>> GetChannelVideosAsync(string url)
    {
        string key = _cachingHelper.CacheVideoList(url);

        var output = _cache.Get<List<VideoModel>>(key);
        if(output is null)
        {
            var channelVideos = await _client.Channels.GetUploadsAsync(url);

            output = channelVideos
                .Select(v => new VideoModel(v))
                .ToList();

            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<VideoModel> GetVideoAsync(string url)
    {
        string key = _cachingHelper.CacheMainVideoKey(url);
        string secondaryKey = _cachingHelper.CacheVideoKey(url); // Used in DownloadHelper

        var output = _cache.Get<VideoModel>(key);
        if (output is null)
        {
            var video = await _client.Videos.GetAsync(url);
            output = new VideoModel(video);

            _cache.Set(key, output, TimeSpan.FromHours(5));
            _cache.Set(secondaryKey, video, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<ChannelModel> GetChannelAsync(string url)
    {
        string key = _cachingHelper.CacheChannelKey(url);

        var output = _cache.Get<ChannelModel>(key);
        if (output is null)
        {
            var channel = await _client.Channels.GetAsync(url);
            output = new ChannelModel(channel);

            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<PlaylistModel> GetPlaylistAsync(string url)
    {
        string key = _cachingHelper.CachePlaylistKey(url);

        var output = _cache.Get<PlaylistModel>(key);
        if (output is null)
        {
            var playlist = await _client.Playlists.GetAsync(url);
            output = new PlaylistModel(playlist);

            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<List<VideoModel>> GetVideosBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"VideoSearch-{searchInput}";

        var output = _cache.Get<List<VideoModel>>(key);
        if (output is null)
        {
            var results = await _client.Search.GetVideosAsync(searchInput, token);
            output = results.Select(v => new VideoModel(v))
                .Take(MaxDataAmount)
                .ToList();
        }

        return output;
    }

    public async Task<List<ChannelModel>> GetChannelBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"ChannelSearch-{searchInput}";

        var output = _cache.Get<List<ChannelModel>>(key);
        if (output is null)
        {
            var results = await _client.Search.GetChannelsAsync(searchInput, token);
            output = results.Select(c => new ChannelModel(c))
                .Take(MaxDataAmount)
                .ToList();
        }

        return output;
    }

    public async Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"PlaylistSearch-{searchInput}";

        var output = _cache.Get<List<PlaylistModel>>(key);
        if (output is null)
        {
            var results = await _client.Search.GetPlaylistsAsync(searchInput, token);
            output = results.Select(p => new PlaylistModel(p))
                .Take(MaxDataAmount)
                .ToList();
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
