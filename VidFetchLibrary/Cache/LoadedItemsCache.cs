using Microsoft.Extensions.Caching.Memory;

namespace VidFetchLibrary.Cache;
public class LoadedItemsCache : ILoadedItemsCache
{
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;

    public LoadedItemsCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public int GetLoadedItemsCount(string page, int defaultValue)
    {
        string cacheKey = CacheItemsCountKey(page);

        int loadedItems = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            return defaultValue;
        });

        return loadedItems;
    }

    public void SetLoadedItemsCount(string page, int loadedItems)
    {
        string cacheKey = CacheItemsCountKey(page);
        _cache.Set(cacheKey, loadedItems, TimeSpan.FromHours(CacheTime));
    }

    private static string CacheItemsCountKey(string page)
    {
        return $"{page}-LoadedItemsCount";
    }
}
