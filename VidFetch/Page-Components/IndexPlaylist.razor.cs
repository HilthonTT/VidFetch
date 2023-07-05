using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexPlaylist
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const string PageName = nameof(IndexPlaylist);
    private const int ItemsPerPage = 6;

    private CancellationTokenSource _tokenSource;
    private List<PlaylistModel> _visiblePlaylists = new();
    private string _playlistSearchText = "";
    private string _playlistUrl = "";
    private int _loadedItems = 6;

    protected override void OnInitialized()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);

        _visiblePlaylists = videoLibrary.Playlists.Take(_loadedItems).ToList();
    }

    private void LoadMorePlaylists()
    {
        int playlistCount = videoLibrary.Playlists.Count;

        _loadedItems += ItemsPerPage;

        if (_loadedItems > playlistCount)
        {
            _loadedItems = playlistCount;
        }

        _visiblePlaylists = videoLibrary.Playlists
            .Take(_loadedItems)
            .ToList();

        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
    }

    private async Task LoadPlaylist()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_playlistUrl))
            {
                return;
            }

            if (Uri.IsWellFormedUriString(_playlistUrl, UriKind.Absolute))
            {
                await OpenLoading.InvokeAsync(true);

                var playlist = await youtube.GetPlaylistAsync(_playlistUrl);

                if (IsPlaylistNull(playlist))
                {
                    videoLibrary.Playlists.Add(playlist);

                    _visiblePlaylists = videoLibrary.Playlists.Take(_loadedItems).ToList();
                }

                await OpenLoading.InvokeAsync(false);
            }
            else
            {
                snackbar.Add("Invalid Url", Severity.Error);
            }

            _playlistUrl = "";
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task<IEnumerable<string>> SearchPlaylists(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Playlists, searchInput);
    }

    private async Task SaveAllPlaylist()
    {
        try
        {
            var playlistsCopy = videoLibrary.Playlists.ToList();
            var token = tokenHelper.InitializeToken(ref _tokenSource);

            foreach (var p in playlistsCopy)
            {
                token.ThrowIfCancellationRequested();
                await playlistData.SetPlaylistAsync(p.Url, p.PlaylistId);
            }


            CancelSavePlaylist();
            snackbar.Add("Successfully saved playlists.");
        }
        catch
        {
            snackbar.Add($"An error occured while saving playlists", Severity.Error);
        }
    }

    private void HandleSearchValueChanged(string value)
    {
        _playlistSearchText = value;
        FilterPlaylists();
    }

    private void FilterPlaylists()
    {
        videoLibrary.Playlists = searchHelper
            .FilterList(videoLibrary.Playlists, _playlistSearchText);

        _visiblePlaylists = searchHelper
            .FilterList(videoLibrary.Playlists, _playlistSearchText)
            .Take(_loadedItems)
            .ToList();
    }

    private void ClearPlaylists()
    {
        videoLibraryHelper.ClearPlaylists();
        _visiblePlaylists.Clear();
    }

    private void RemovePlaylist(PlaylistModel playlist)
    {
        videoLibraryHelper.RemovePlaylist(playlist);
        _visiblePlaylists.Remove(playlist);
    }

    private void CancelSavePlaylist()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
    }

    private string GetSearchBarText()
    {
        if (videoLibrary?.Playlists.Count <= 0)
        {
            return "Search Playlist";
        }

        if (videoLibrary.Playlists?.Count == 1)
        {
            return "Search 1 Playlist";
        }

        return $"Search {videoLibrary.Playlists?.Count} Playlists";
    }

    private bool IsPlaylistNull(PlaylistModel playlist)
    {
        return videoLibrary.Playlists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId) is null;
    }
}