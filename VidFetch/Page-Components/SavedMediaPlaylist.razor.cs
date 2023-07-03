using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SavedMediaPlaylist
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private CancellationTokenSource _tokenSource;
    private List<PlaylistModel> _playlists = new();
    private List<PlaylistModel> _visiblePlaylists = new();
    private string _searchText = "";
    private int _loadedItems = 6;
    private bool _isVisible = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadPlaylists();
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

    private async Task UpdateAllPlaylists()
    {
        try
        {
            var playlistCopy = _playlists.ToList();
            var token = tokenHelper.InitializeToken(ref _tokenSource);

            foreach (var p in playlistCopy)
            {
                token.ThrowIfCancellationRequested();
                await UpdatePlaylist(p, token);
            }

            CancelUpdatePlaylist();
            snackbar.Add($"Successfully updated.");
        }
        catch
        {
            snackbar.Add($"An error occured while updating.", Severity.Error);
        }
    }

    private async Task UpdatePlaylist(PlaylistModel playlist, CancellationToken token)
    {
        var newPlaylist = await youtube.GetPlaylistAsync(playlist.Url, token);

        if (newPlaylist is null)
        {
            RemovePlaylist(playlist);
            snackbar.Add($"{playlist.Title} no longer exists. It has been deleted", Severity.Error);
            await playlistData.DeletePlaylistAsync(playlist);
        }
        else
        {
            await playlistData.SetPlaylistAsync(playlist.Url, playlist.PlaylistId);
        }
    }

    private async Task DeleteAllPlaylists()
    {
        CloseDialog();
        
        var playlistsCopy = _playlists.ToList();

        foreach (var p in playlistsCopy)
        {
            RemovePlaylist(p);
            await playlistData.DeletePlaylistAsync(p);
        }
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterPlaylists();
    }

    private void FilterPlaylists()
    {
        _playlists = searchHelper
            .FilterList(_playlists, _searchText);

        _visiblePlaylists = searchHelper
            .FilterList(_playlists, _searchText)
            .Take(_loadedItems)
            .ToList();
    }

    private void CancelUpdatePlaylist()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
    }

    private void RemovePlaylist(PlaylistModel playlist)
    {
        _visiblePlaylists?.Remove(playlist);
        _playlists?.Remove(playlist);
    }

    private void OpenDialog()
    {
        _isVisible = true;
    }

    private void CloseDialog()
    {
        _isVisible = false;
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