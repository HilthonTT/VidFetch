using Microsoft.Extensions.Caching.Memory;
using System.Web;
using VidFetchLibrary.Helpers;
using VidFetchLibrary.Models;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace VidFetchLibrary.Client;
public class Youtube : IYoutube
{
    private const int MaxDataCount = 50;
    private const int CacheTime = 5;

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
        IProgress<double> progress,
        CancellationToken token,
        bool isPlaylist = false,
        string playlistTitle = "")
    { 
        var youtube = new YoutubeClient();

        await _downloaderHelper.DownloadVideoAsync(
            youtube,
            url,
            progress,
            token,
            isPlaylist,
            playlistTitle);
    }

    public async Task<List<VideoModel>> GetPlayListVideosAsync(string url)
    {
        string key = _cachingHelper.CacheVideoList(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");
            var playlistVideos = await _client.Playlists.GetVideosAsync(playlistId)
                .CollectAsync(MaxDataCount);

            return playlistVideos
                .Select(v => new VideoModel(v))
                .ToList();
        });

        return output;
    }

    public async Task<List<VideoModel>> GetChannelVideosAsync(string url)
    {
        string key = _cachingHelper.CacheVideoList(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var channelVideos = await _client.Channels.GetUploadsAsync(url)
                .CollectAsync(MaxDataCount);

            return channelVideos
                .Select(v => new VideoModel(v))
                .ToList();
        });

        return output;
    }

    public async Task<VideoModel> GetVideoAsync(string url)
    {
        string key = _cachingHelper.CacheMainVideoKey(url);
        string secondaryKey = _cachingHelper.CacheVideoKey(url); // Used in DownloadHelper

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var video = await _client.Videos.GetAsync(url);

            _cache.Set(secondaryKey, video, TimeSpan.FromHours(5));
            return new VideoModel(video);
        });

        return output;
    }

    public async Task<ChannelModel> GetChannelAsync(string url)
    {
        string key = _cachingHelper.CacheChannelKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            if (url.Contains("https://www.youtube.com/@"))
            {
                var channel = await _client.Channels.GetByHandleAsync(url);
                return new ChannelModel(channel);
            }
            else if (url.Contains("https://youtube.com/user/"))
            {
                var channel = await _client.Channels.GetByUserAsync(url);
                return new ChannelModel(channel);
            }
            else if (url.Contains("https://youtube.com/c/"))
            {
                var channel = await _client.Channels.GetBySlugAsync(url);
                return new ChannelModel(channel);
            }
            else
            {
                var channel = await _client.Channels.GetAsync(url);
                return new ChannelModel(channel);
            }
        });

        return output;
    }

    public async Task<PlaylistModel> GetPlaylistAsync(string url)
    {
        string key = _cachingHelper.CachePlaylistKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var playlist = await _client.Playlists.GetAsync(url);
            return new PlaylistModel(playlist);
        });
        
        return output;
    }

    public async Task<List<VideoModel>> GetVideosBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"VideoSearch-{searchInput}";

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _client.Search.GetVideosAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results.Select(v => new VideoModel(v))
                .ToList();
        });

        return output;
    }

    public async Task<List<ChannelModel>> GetChannelBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"ChannelSearch-{searchInput}";

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _client.Search.GetChannelsAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results.Select(v => new ChannelModel(v))
                .ToList();
        });

        return output;
    }

    public async Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        string key = $"PlaylistSearch-{searchInput}";

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _client.Search.GetPlaylistsAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results.Select(v => new PlaylistModel(v))
                .ToList();
        });

        return output;
    }

    private static string GetPlaylistId(string url) 
    {
        string queryString = new Uri(url).Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);
        return queryParams.Get("list");
    }
}
