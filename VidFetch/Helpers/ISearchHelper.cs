namespace VidFetch.Helpers;

public interface ISearchHelper<TData> where TData : class
{
    List<T> FilterList<T>(List<T> items, string searchText);
    Task<List<TData>> GetBySearchAsync(string url, int count, CancellationToken token);
    Task<IEnumerable<string>> SearchAsync<T>(List<T> items, string searchInput);
}