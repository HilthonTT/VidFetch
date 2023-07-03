using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SearchPlaylist
{
    private List<PlaylistModel> _visiblePlaylists = new();
    private CancellationTokenSource _playlistTokenSource;
    private string _playlistUrl = "";
    private string _searchText = "";
    private int _loadedItems = 6;

    protected override void OnInitialized()
    {
        _visiblePlaylists = videoLibrary.PlaylistResults
            .Take(_loadedItems)
            .ToList();
    }

    private void LoadMorePlaylists()
    {
        int itemsPerPage = 6;
        int playlistCount = videoLibrary.PlaylistResults.Count;

        _loadedItems += itemsPerPage;

        if (_loadedItems > playlistCount)
        {
            _loadedItems = playlistCount;
        }

        _visiblePlaylists = videoLibrary.PlaylistResults
            .Take(_loadedItems)
            .ToList();
    }

    private async Task SearchPlaylists()
    {
        if (string.IsNullOrWhiteSpace(_playlistUrl)is false)
        {
            var token = tokenHelper.InitializeToken(ref _playlistTokenSource);

            videoLibrary.PlaylistResults = await youtube.GetPlaylistsBySearchAsync(_playlistUrl, token);

            _visiblePlaylists = videoLibrary.PlaylistResults
                .Take(_loadedItems)
                .ToList();

            CancelPlaylistSearch();
        }
    }

    private async Task<IEnumerable<string>> FilterSearchPlaylists(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.PlaylistResults, searchInput);
    }

    private void HandlePlaylistSearchValueChanged(string value)
    {
        _searchText = value;
        FilterPlaylists();
    }

    private void FilterPlaylists()
    {
        videoLibrary.PlaylistResults = searchHelper
            .FilterList(videoLibrary.PlaylistResults, _searchText);

        _visiblePlaylists = searchHelper
            .FilterList(videoLibrary.PlaylistResults, _searchText)
            .Take(_loadedItems)
            .ToList();
    }

    private void ClearPlaylists()
    {
        videoLibrary.PlaylistResults?.Clear();
    }

    private void RemovePlaylist(PlaylistModel playlist)
    {
        videoLibrary.PlaylistResults.Remove(playlist);
        _visiblePlaylists.Remove(playlist);
    }

    private void CancelPlaylistSearch()
    {
        tokenHelper.CancelRequest(ref _playlistTokenSource);
    }

    private string GetPlaylistSearchBarText()
    {
        if (videoLibrary.PlaylistResults?.Count <= 0)
        {
            return "Search Playlist";
        }

        if (videoLibrary.PlaylistResults?.Count == 1)
        {
            return "Search 1 Playlist";
        }

        return $"Search {videoLibrary.PlaylistResults?.Count} Playlists";
    }
}