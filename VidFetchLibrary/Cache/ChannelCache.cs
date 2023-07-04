using Microsoft.Extensions.Caching.Memory;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Models;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace VidFetchLibrary.Cache;
public class ChannelCache : IChannelCache
{
    private const int CacheTime = 5;
    private const int MaxDataCount = 200;
    private readonly IMemoryCache _cache;
    private readonly ChannelClient _channelClient;
    private readonly SearchClient _searchClient;

    public ChannelCache(IMemoryCache cache,
                        ChannelClient channelClient,
                        SearchClient searchClient)
    {
        _cache = cache;
        _channelClient = channelClient;
        _searchClient = searchClient;
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
                bool _ when url.Contains("https://www.youtube.com/@") => await _channelClient.GetByHandleAsync(url, token),
                bool _ when url.Contains("https://youtube.com/user/") => await _channelClient.GetByUserAsync(url, token),
                bool _ when url.Contains("https://youtube.com/c/") => await _channelClient.GetBySlugAsync(url, token),
                _ => await _channelClient.GetAsync(url, token),
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

            var results = await _searchClient.GetChannelsAsync(searchInput, token)
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

    private static string CacheChannelSearch(string searchInput)
    {
        return $"{nameof(ChannelCache)}-{searchInput}";
    }

    private static string CacheChannelKey(string url)
    {
        return $"{nameof(ChannelData)}-{url}";
    }
}
