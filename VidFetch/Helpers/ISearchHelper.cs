namespace VidFetch.Helpers;

public interface ISearchHelper
{
    List<T> FilterList<T>(List<T> items, string searchText);
    Task<IEnumerable<string>> SearchAsync<T>(List<T> videos, string searchInput);
}