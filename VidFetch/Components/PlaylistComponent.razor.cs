using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Models;

namespace VidFetch.Components;

public partial class PlaylistComponent
{
    [Parameter]
    [EditorRequired]
    public PlaylistModel Playlist { get; set; }

    [Parameter]
    public int CardSize { get; set; } = 12;

    [Parameter]
    [EditorRequired]
    public EventCallback<PlaylistModel> RemoveEvent { get; set; }

    private bool _isSaved = false;
    
    protected override async Task OnInitializedAsync()
    {
        _isSaved = await playlistData.PlaylistExistsAsync(Playlist.Url, Playlist.PlaylistId);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadPlaylistData();
        }
    }

    private async Task LoadPlaylistData()
    {
        var videoTask = new List<Task>
        {
            LoadThumbnail(),
            LoadAuthorThumbnail(),
        };

        await Task.WhenAll(videoTask);
    }

    private async Task SavePlaylist()
    {
        if (_isSaved is false)
        {
            await playlistData.SetPlaylistAsync(Playlist.Url, Playlist.PlaylistId);

            snackbar.Add($"Successfully saved {Playlist.Title}");
            _isSaved = true;
        }
    }

    private async Task LoadAuthorThumbnail()
    {
        bool isAuthorThumbnailEmpty = string.IsNullOrWhiteSpace(Playlist.AuthorThumbnailUrl);

        if (isAuthorThumbnailEmpty)
        {
            var channel = await youtube.GetChannelAsync(Playlist.AuthorUrl);
            string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? "" : channel.ThumbnailUrl;
            Playlist.AuthorThumbnailUrl = channelThumbnail;
        }
    }

    private async Task LoadThumbnail()
    {
        bool isThumbnailEmpty = string.IsNullOrWhiteSpace(Playlist.ThumbnailUrl);

        if (isThumbnailEmpty)
        {
            var playlist = await youtube.GetPlaylistAsync(Playlist.Url);
            string playlistThumbnail = string.IsNullOrWhiteSpace(playlist.ThumbnailUrl) ? "" : playlist.ThumbnailUrl;
            Playlist.ThumbnailUrl = playlistThumbnail;
        }
    }

    private async Task Remove()
    {
        await RemoveEvent.InvokeAsync(Playlist);
    }

    private async Task OpenUrl(string text)
    {
        await launcher.OpenAsync(text);
    }

    private void LoadPlaylistPage()
    {
        string encodedUrl = Uri.EscapeDataString(Playlist.Url);
        navManager.NavigateTo($"/Playlist/{encodedUrl}");
    }
}