namespace VidFetchLibrary.Cache;

public interface ILoadedItemsCache
{
    int GetLoadedItemsCount(string page, int defaultValue);
    void SetLoadedItemsCount(string page, int loadedItems);
}