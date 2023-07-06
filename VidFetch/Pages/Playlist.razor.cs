using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using VidFetchLibrary.Language;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Playlist
{
    [Parameter]
    public string Url { get; set; }

    private PlaylistModel _playlist;
    private List<VideoModel> _videos = new();
    private List<VideoModel> _visibleVideos = new();
    private bool _isSaved = false;
    private string _playlistId = "";
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        _playlistId = UrlRegex().Match(Url).Value;
        await LoadData();

        _visibleVideos = _videos.Take(_loadedItems).ToList();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadNullData();
    }

    private void LoadMoreVideos()
    {
        int itemsPerPage = 6;
        int videosCount = _videos.Count;

        _loadedItems += itemsPerPage;

        if (_loadedItems > videosCount)
        {
            _loadedItems = videosCount;
        }

        _visibleVideos = _videos.Take(_loadedItems).ToList();
    }

    private async Task LoadData()
    {
        bool isUrlValid = Uri.IsWellFormedUriString(Url, UriKind.Absolute);
        if (isUrlValid)
        {
            _playlist = await playlistData.GetPlaylistAsync(Url, _playlistId);
            if (_playlist is null)
            {
                _playlist = await youtube.GetPlaylistAsync(Url);
            }

            _isSaved = await playlistData.PlaylistExistsAsync(_playlist.Url, _playlist.PlaylistId);
            _videos = await youtube.GetPlayListVideosAsync(Url);
        }
    }

    private async Task LoadNullData()
    {
        if (_playlist is null)
        {
            return;
        }

        bool isThumbnailEmpty = string.IsNullOrWhiteSpace(_playlist.ThumbnailUrl);
        bool isAuthorThumbnailEmpty = string.IsNullOrWhiteSpace(_playlist.AuthorThumbnailUrl);
        string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";
        if (isThumbnailEmpty)
        {
            var p = await youtube.GetPlaylistAsync(_playlist.Url);
            string playlistThumbnail = string.IsNullOrWhiteSpace(p.ThumbnailUrl) ? defaultUrl : p.ThumbnailUrl;
            _playlist.ThumbnailUrl = playlistThumbnail;
        }

        if (isAuthorThumbnailEmpty)
        {
            var channel = await youtube.GetChannelAsync(_playlist.AuthorUrl);
            string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? defaultUrl : channel.ThumbnailUrl;
            _playlist.AuthorThumbnailUrl = channelThumbnail;
        }
    }

    private async Task SavePlaylist()
    {
        if (_isSaved is false)
        {
            await playlistData.SetPlaylistAsync(_playlist.Url, _playlist.PlaylistId);
            snackbar.Add($"Successfully saved {_playlist.Title}");
            _isSaved = true;
        }
    }

    private async Task DeletePlaylist()
    {
        if (_isSaved)
        {
            await playlistData.DeletePlaylistAsync(_playlist);
            snackbar.Add($"Successfully deleted {_playlist.Title}");
            _isSaved = false;

            navManager.NavigateTo("/SavedMedias");
        }
    }

    private async Task OpenUrl()
    {
        await launcher.OpenAsync(Url);
    }

    private bool IsPlaylistNull()
    {
        if (_playlist is null)
        {
            return true;
        }

        return false;
    }

    private void RemoveVideo(VideoModel video)
    {
        _videos.Remove(video);
        _visibleVideos.Remove(video);
    }

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = languageExtensions.GetDictionary();
        return dictionary;
    }

    [GeneratedRegex("list=([A-Za-z0-9_-]+)")]
    private static partial Regex UrlRegex();
}