
namespace VidFetch.Helpers;
public class SearchHelper : ISearchHelper
{
    public async Task<IEnumerable<string>> SearchAsync<T>(List<T> items, string searchInput)
    {
        return await Task.Run(() =>
        {
            var output = items;
            if (string.IsNullOrWhiteSpace(searchInput) is false)
            {
                var titleProperty = typeof(T).GetProperty("Title");

                if (titleProperty is not null)
                {
                    output = output
                        .Where(v => titleProperty.GetValue(v)?.ToString()
                            .Contains(searchInput, StringComparison.InvariantCultureIgnoreCase) ?? false)
                        .ToList();
                }
            }

            return output.Select(v =>
            {
                var title = typeof(T).GetProperty("Title")?.GetValue(v)?.ToString();
                return title ?? string.Empty;
            });
        });
    }

    public List<T> FilterList<T>(List<T> items, string searchText)
    {
        var output = items;

        if (string.IsNullOrWhiteSpace(searchText) is false)
        {
            var titleProperty = typeof(T).GetProperty("Title");

            output = output
                .OrderByDescending(v => titleProperty.GetValue(v)?.ToString()
                    .Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        return output;
    }
}
