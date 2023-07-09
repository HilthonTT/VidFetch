using VidFetchLibrary.Data;

namespace VidFetch.Helpers;
public interface IVideoLibraryFilterHelper<TData> where TData : class
{
    void ClearListData(VideoLibraryList list);
    List<TData> FilterData(VideoLibraryList list, string searchText, int loadedItems);
    Task<IEnumerable<string>> FilterSearchData(VideoLibraryList list, string searchInput);
    List<TData> GetDataResults(VideoLibraryList list);
    void RemoveData(VideoLibraryList list, TData data);
}