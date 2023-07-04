using Microsoft.Extensions.Caching.Memory;
using System.Web;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Models;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Cache;
public class VideoCache : IVideoCache
{
    private const int CacheTime = 5;
    private const int MaxDataCount = 200;
    private readonly IMemoryCache _cache;
    private readonly YoutubeClient _youtube;

    public VideoCache(IMemoryCache cache,
                      YoutubeClient youtube)
    {
        _cache = cache;
        _youtube = youtube;
    }

    public async Task<VideoModel> GetVideoAsync(
        string url,
        CancellationToken token = default)
    {
        string primaryKey = CachePrimaryVideoKey(url);
        string secondaryKey = CacheSecondaryVideoKey(url);

        var output = await _cache.GetOrCreateAsync(primaryKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var video = await _youtube.Videos.GetAsync(url, token);

            _cache.Set(secondaryKey, video, TimeSpan.FromHours(CacheTime));
            return new VideoModel(video);
        });

        if (output is null)
        {
            _cache.Remove(primaryKey);
            _cache.Remove(secondaryKey);
        }

        return output;
    }

    public async Task<Video> LoadYoutubeExplodeVideoAsync(
        string url,
        CancellationToken token = default)
    {
        string key = CacheSecondaryVideoKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            
            return await _youtube.Videos.GetAsync(url, token);
        });

        if (output is null)
        {
            _cache.Remove(key);
        }

        return output;
    }

    public async Task<List<VideoModel>> GetPlayListVideosAsync(
        string url,
        CancellationToken token = default)
    {
        string key = CacheVideoList(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");
            var playlistVideos = await _youtube.Playlists.GetVideosAsync(playlistId, token)
                .CollectAsync(MaxDataCount);

            return playlistVideos
                .Select(v => new VideoModel(v))
                .ToList();
        });

        if (output is null)
        {
            _cache.Remove(key);
        }

        return output;
    }

    public async Task<List<VideoModel>> GetChannelVideosAsync(
        string url,
        CancellationToken token = default)
    {
        string key = CacheVideoList(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var channelVideos = await _youtube.Channels.GetUploadsAsync(url, token)
                .CollectAsync(MaxDataCount);

            return channelVideos
                .Select(v => new VideoModel(v))
                .ToList();
        });

        if (output is null)
        {
            _cache.Remove(key);
        }

        return output;
    }

    public async Task<List<VideoModel>> GetVideosBySearchAsync(
        string searchInput,
        CancellationToken token = default)
    {
        string key = CacheVideoSearch(searchInput);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _youtube.Search.GetVideosAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results.Select(v => new VideoModel(v))
                .ToList();
        });

        if (output is null)
        {
            _cache.Remove(key);
        }

        return output;
    }

    public string CachePrimaryVideoKey(string url)
    {
        string cacheName = nameof(VideoData);

        return $"{cacheName}-{url}";
    }

    public string CacheSecondaryVideoKey(string url)
    {
        string cacheName = nameof(Video);
        
        return $"{cacheName}-{url}";
    }

    private static string CacheVideoSearch(string searchInput)
    {
        string cacheName = nameof(VideoCache);

        return $"{cacheName}-{searchInput}";
    }

    private static string GetPlaylistId(string url)
    {
        string queryString = new Uri(url).Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);

        return queryParams.Get("list");
    }

    private static string CacheVideoList(string url)
    {
        string cacheName = nameof(List<VideoModel>);

        return $"{cacheName}-{url}";
    }
}
