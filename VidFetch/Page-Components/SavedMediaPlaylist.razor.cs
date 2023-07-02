using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SavedMediaPlaylist
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private List<PlaylistModel> _playlists = new();
    private List<PlaylistModel> _visiblePlaylists = new();
    private string _searchText = "";
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        await LoadPlaylists();
    }

    private async Task LoadPlaylists()
    {
        await OpenLoading.InvokeAsync(true);
        _playlists = await playlistData.GetAllPlaylistsAsync();
        _visiblePlaylists = _playlists.Take(_loadedItems).ToList();
        await OpenLoading.InvokeAsync(false);
    }

    private async Task DeletePlaylist(PlaylistModel playlist)
    {
        _playlists.Remove(playlist);
        _visiblePlaylists.Remove(playlist);
        await playlistData.DeletePlaylistAsync(playlist);
    }

    private async Task<IEnumerable<string>> SearchPlaylists(string searchInput)
    {
        return await searchHelper.SearchAsync(_playlists, searchInput);
    }

    private void FilterPlaylists()
    {
        _playlists = searchHelper.FilterList(_playlists, _searchText);
        _visiblePlaylists = searchHelper.FilterList(_playlists, _searchText).Take(_loadedItems).ToList();
    }

    private void LoadMorePlaylists()
    {
        int itemsPerPage = 6;
        int playlistCount = _playlists.Count;
        _loadedItems += itemsPerPage;
        if (_loadedItems > playlistCount)
        {
            _loadedItems = playlistCount;
        }

        _visiblePlaylists = _playlists.Take(_loadedItems).ToList();
    }

    private string GetSearchBarText()
    {
        if (_playlists?.Count <= 0)
        {
            return "Search Playlist";
        }

        if (_playlists?.Count == 1)
        {
            return "Search 1 Playlist";
        }

        return $"Search {_playlists?.Count} Playlists";
    }
}