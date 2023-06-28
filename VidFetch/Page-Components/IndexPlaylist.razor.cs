using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexPlaylist
{
    private string _playlistSearchText = "";
    private bool _isPlaylistLoading = false;
    private string _playlistUrl = "";
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
                _isPlaylistLoading = true;
                var playlist = await youtube.GetPlaylistAsync(_playlistUrl);
                bool playlistIsNull = videoLibrary.Playlists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId)is null;
                if (playlistIsNull)
                {
                    videoLibrary.Playlists.Add(playlist);
                }

                _isPlaylistLoading = false;
            }
            else
            {
                snackbar.Add("Invalid Url", Severity.Warning);
            }

            _playlistUrl = "";
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
            _isPlaylistLoading = false;
        }
    }

    private async Task<IEnumerable<string>> SearchPlaylists(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Playlists, searchInput);
    }

    private void FilterPlaylists()
    {
        videoLibrary.Playlists = searchHelper.FilterList(videoLibrary.Playlists, _playlistSearchText);
    }

    private void ClearPlaylists()
    {
        videoLibraryHelper.ClearPlaylists();
    }

    private void RemovePlaylist(PlaylistModel playlist)
    {
        var p = videoLibrary.Playlists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId || p.Url == playlist.Url);
        videoLibraryHelper.RemovePlaylist(p);
    }

    private string GetPlaylistSearchBarText()
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
}