
using VidFetchLibrary.Client;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Helpers;
public class SearchHelper<TData> : ISearchHelper<TData> where TData : class
{
    private readonly IYoutube _youtube;
    private readonly IVideoLibrary _videoLibrary;

    public SearchHelper(IYoutube youtube, IVideoLibrary videoLibrary)
    {
        _youtube = youtube;
        _videoLibrary = videoLibrary;
    }

    public async Task<List<TData>> GetBySearchAsync(string url, CancellationToken token)
    {
        return typeof(TData) switch
        {
            Type video when video == typeof(VideoModel) => await GetVideosBySearchAsync(url, token),
            Type channel when channel == typeof(ChannelModel) => await GetChannelsBySearchAsync(url, token),
            Type playlist when playlist == typeof(PlaylistModel) => await GetPlaylistsBySearchAsync(url, token),
            _ => default,
        };
    }

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

    private async Task<List<TData>> GetVideosBySearchAsync(string url, CancellationToken token)
    {
        _videoLibrary.VideoResults = await _youtube.GetVideosBySearchAsync(url, token);
        return _videoLibrary.VideoResults as List<TData>;
    }

    private async Task<List<TData>> GetChannelsBySearchAsync(string url, CancellationToken token)
    {
        _videoLibrary.ChannelResults = await _youtube.GetChannelsBySearchAsync(url, token);
        return _videoLibrary.ChannelResults as List<TData>;
    }

    private async Task<List<TData>> GetPlaylistsBySearchAsync(string url, CancellationToken token)
    {
        _videoLibrary.PlaylistResults = await _youtube.GetPlaylistsBySearchAsync(url, token);
        return _videoLibrary.PlaylistResults as List<TData>;
    }
}
