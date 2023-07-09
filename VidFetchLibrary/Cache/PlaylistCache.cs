using Microsoft.Extensions.Caching.Memory;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Models;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace VidFetchLibrary.Cache;
public class PlaylistCache : IPlaylistCache
{
    private const int CacheTime = 5;
    private const int MaxDataCount = 200;
    private readonly IMemoryCache _cache;
    private readonly YoutubeClient _youtube;

    public PlaylistCache(IMemoryCache cache, YoutubeClient youtube)
    {
        _cache = cache;
        _youtube = youtube;
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

    public async Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(
        string searchInput,
        CancellationToken token = default)
    {
        string key = CachePlaylistSearch(searchInput);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _youtube.Search.GetPlaylistsAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results;
        });

        if (output is null)
        {
            _cache.Remove(key);
            return new();
        }

        var playlists = output.Select(p => new PlaylistModel(p)).ToList();
        return playlists;
    }

    private static string CachePlaylistSearch(string searchInput)
    {
        string cacheName = nameof(PlaylistCache);

        return $"{cacheName}-{searchInput}";
    }

    private static string CachePlaylistKey(string url)
    {
        string cacheName = nameof(PlaylistData);

        return $"{cacheName}-{url}";
    }
}
