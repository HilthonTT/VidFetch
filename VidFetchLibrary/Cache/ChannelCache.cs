using Microsoft.Extensions.Caching.Memory;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Models;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;

namespace VidFetchLibrary.Cache;
public class ChannelCache : IChannelCache
{
    private const int CacheTime = 5;
    private const int MaxDataCount = 200;
    private readonly IMemoryCache _cache;
    private readonly YoutubeClient _youtube;

    public ChannelCache(IMemoryCache cache,
                        YoutubeClient youtube)
    {
        _cache = cache;
        _youtube = youtube;
    }

    public async Task<ChannelModel> GetChannelAsync(
        string url,
        CancellationToken token = default)
    {
        string key = CacheChannelKey(url);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            
            Channel channel = true switch
            {
                bool _ when url.Contains("https://www.youtube.com/@") => await _youtube.Channels
                    .GetByHandleAsync(url, token),

                bool _ when url.Contains("https://youtube.com/user/") => await _youtube.Channels
                .GetByUserAsync(url, token),

                bool _ when url.Contains("https://youtube.com/c/") => await _youtube.Channels
                .GetBySlugAsync(url, token),

                _ => await _youtube.Channels.GetAsync(url, token),
            };
            
            return new ChannelModel(channel);
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
        string key = CacheChannelSearch(searchInput);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var results = await _youtube.Search
               .GetChannelsAsync(searchInput, token)
               .CollectAsync(MaxDataCount);

            return results;
        });

        if (output is null)
        {
            _cache.Remove(key);
            return new();
        }

        var channels = output.Select(c => new ChannelModel(c)).ToList();
        return channels;
    }

    private static string CacheChannelSearch(string searchInput)
    {
        return $"{nameof(ChannelCache)}-{searchInput}";
    }

    private static string CacheChannelKey(string url)
    {
        return $"{nameof(ChannelData)}-{url}";
    }
}
