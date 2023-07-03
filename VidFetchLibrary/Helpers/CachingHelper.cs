using Microsoft.Extensions.Caching.Memory;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Models;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using System.Web;
using VidFetchLibrary.Library;

namespace VidFetchLibrary.Helpers;
public class CachingHelper : ICachingHelper
{
    private const int CacheTime = 5;
    private const int MaxDataCount = 200;
    private readonly IMemoryCache _cache;
    private readonly ISettingsLibrary _settings;
    private YoutubeClient _youtube;

    public CachingHelper(IMemoryCache cache, ISettingsLibrary settings)
    {
        _cache = cache;
        _settings = settings;
        InitializeClient();
    }

    private void InitializeClient()
    {
        _youtube = new();
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

    public async Task<ChannelModel> GetChannelAsync(
        string url,
        CancellationToken token = default)
    {
        string key = CacheChannelKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            if (url.Contains("https://www.youtube.com/@"))
            {
                var channel = await _youtube.Channels.GetByHandleAsync(url, token);
                return new ChannelModel(channel);
            }
            else if (url.Contains("https://youtube.com/user/"))
            {
                var channel = await _youtube.Channels.GetByUserAsync(url, token);
                return new ChannelModel(channel);
            }
            else if (url.Contains("https://youtube.com/c/"))
            {
                var channel = await _youtube.Channels.GetBySlugAsync(url, token);
                return new ChannelModel(channel);
            }
            else
            {
                var channel = await _youtube.Channels.GetAsync(url, token);
                return new ChannelModel(channel);
            }
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

    public async Task<PlaylistModel> GetPlaylistAsync(
        string url,
        CancellationToken token = default)
    {
        string key = CachePlaylistKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var playlist = await _youtube.Playlists.GetAsync(url, token);
            return new PlaylistModel(playlist);
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
        string key = $"VideoSearch-{searchInput}";

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

    public async Task<List<ChannelModel>> GetChannelBySearchAsync(
        string searchInput,
        CancellationToken token = default)
    {
        string key = $"ChannelSearch-{searchInput}";

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _youtube.Search.GetChannelsAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results.Select(v => new ChannelModel(v))
                .ToList();
        });

        if (output is null)
        {
            _cache.Remove(key);
        }

        return output;
    }

    public async Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(
        string searchInput,
        CancellationToken token = default)
    {
        string key = $"PlaylistSearch-{searchInput}";

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _youtube.Search.GetPlaylistsAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results.Select(v => new PlaylistModel(v))
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
        return $"{nameof(VideoData)}-{url}";
    }

    public string CacheSecondaryVideoKey(string url)
    {
        return $"{nameof(Video)}-{url}";
    }

    public string CacheStreamManifest(string id)
    {
        return $"{nameof(StreamManifest)}-{id}";
    }

    public string CacheSubtitlesInfoKey(string id)
    {
        return $"{nameof(ClosedCaptionTrackInfo)}-{id}";
    }

    private static string GetPlaylistId(string url)
    {
        string queryString = new Uri(url).Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);

        return queryParams.Get("list");
    }

    private static string CacheVideoList(string url)
    {
        return $"{nameof(List<VideoModel>)}-{url}";
    }

    private static string CacheChannelKey(string url)
    {
        return $"{nameof(ChannelData)}-{url}";
    }

    private static string CachePlaylistKey(string url)
    {
        return $"{nameof(PlaylistData)}-{url}";
    }
}
