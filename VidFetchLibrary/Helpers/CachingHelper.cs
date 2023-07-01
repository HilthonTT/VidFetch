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

    public async Task<VideoModel> GetVideoAsync(string url)
    {
        string primaryKey = CachePrimaryVideoKey(url);
        string secondaryKey = CacheSecondaryVideoKey(url);

        var output = await _cache.GetOrCreateAsync(primaryKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var video = await _youtube.Videos.GetAsync(url);

            _cache.Set(secondaryKey, video, TimeSpan.FromHours(CacheTime));
            return new VideoModel(video);
        });

        return output;
    }

    public async Task<Video> LoadYoutubeExplodeVideoAsync(
        string url,
        CancellationToken token)
    {
        string key = CacheSecondaryVideoKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            return await _youtube.Videos.GetAsync(url, token);
        });

        return output;
    }

    public async Task<ChannelModel> GetChannelAsync(string url)
    {
        string key = CacheChannelKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            if (url.Contains("https://www.youtube.com/@"))
            {
                var channel = await _youtube.Channels.GetByHandleAsync(url);
                return new ChannelModel(channel);
            }
            else if (url.Contains("https://youtube.com/user/"))
            {
                var channel = await _youtube.Channels.GetByUserAsync(url);
                return new ChannelModel(channel);
            }
            else if (url.Contains("https://youtube.com/c/"))
            {
                var channel = await _youtube.Channels.GetBySlugAsync(url);
                return new ChannelModel(channel);
            }
            else
            {
                var channel = await _youtube.Channels.GetAsync(url);
                return new ChannelModel(channel);
            }
        });

        return output;
    }

    public async Task<List<VideoModel>> GetPlayListVideosAsync(string url)
    {
        string key = CacheVideoList(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");
            var playlistVideos = await _youtube.Playlists.GetVideosAsync(playlistId)
                .CollectAsync(MaxDataCount);

            return playlistVideos
                .Select(v => new VideoModel(v))
                .ToList();
        });

        return output;
    }

    public async Task<List<VideoModel>> GetChannelVideosAsync(string url)
    {
        string key = CacheVideoList(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var channelVideos = await _youtube.Channels.GetUploadsAsync(url)
                .CollectAsync(MaxDataCount);

            return channelVideos
                .Select(v => new VideoModel(v))
                .ToList();
        });

        return output;
    }

    public async Task<PlaylistModel> GetPlaylistAsync(string url)
    {
        string key = CachePlaylistKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var playlist = await _youtube.Playlists.GetAsync(url);
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

            var results = await _youtube.Search.GetVideosAsync(searchInput, token)
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

            var results = await _youtube.Search.GetChannelsAsync(searchInput, token)
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

            var results = await _youtube.Search.GetPlaylistsAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results.Select(v => new PlaylistModel(v))
                .ToList();
        });

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
