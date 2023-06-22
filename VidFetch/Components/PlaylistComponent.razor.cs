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

    private bool isSaved = false;
    protected override async Task OnInitializedAsync()
    {
        isSaved = await playlistData.PlaylistExistsAsync(Playlist.Url, Playlist.PlaylistId);
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
    }

    private async Task SavePlaylist()
    {
        if (isSaved is false)
        {
            await playlistData.SetPlaylistAsync(Playlist.Url, Playlist.PlaylistId);
            snackbar.Add($"Successfully saved {Playlist.Title}");
            isSaved = true;
        }
    }

    private async Task LoadNullData()
    {
        bool isThumbnailEmpty = string.IsNullOrWhiteSpace(Playlist.ThumbnailUrl);
        bool isAuthorThumbnailEmpty = string.IsNullOrWhiteSpace(Playlist.AuthorThumbnailUrl);
        string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";
        if (isThumbnailEmpty)
        {
            var playlist = await youtube.GetPlaylistAsync(Playlist.Url);
            string playlistThumbnail = string.IsNullOrWhiteSpace(playlist.ThumbnailUrl) ? defaultUrl : playlist.ThumbnailUrl;
            Playlist.ThumbnailUrl = playlistThumbnail;
        }

        if (isAuthorThumbnailEmpty)
        {
            var channel = await youtube.GetChannelAsync(Playlist.AuthorUrl);
            string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? defaultUrl : channel.ThumbnailUrl;
            Playlist.AuthorThumbnailUrl = channelThumbnail;
        }
    }

    private async Task Remove()
    {
        await RemoveEvent.InvokeAsync(Playlist);
    }

    private static async Task OpenUrl(string text)
    {
        await Launcher.OpenAsync(text);
    }

    private void LoadPlaylistPage()
    {
        string encodedUrl = Uri.EscapeDataString(Playlist.Url);
        navManager.NavigateTo($"/Playlist/{encodedUrl}");
    }
}