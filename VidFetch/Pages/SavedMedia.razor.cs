using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class SavedMedia
{
    private CancellationTokenSource _allVideosTokenSource;
    private List<VideoModel> _videos = new();
    private List<ChannelModel> _channels = new();
    private List<PlaylistModel> _playlists = new();
    private string _videoSearchText = "";
    private string _channelSearchText = "";
    private string _playlistSearchText = "";
    private string _currentDownloadingVideo = "";
    private double _videosProgress = 0;
    private bool _isVideosLoading = true;
    private bool _isPlaylistsLoading = true;
    private bool _isChannelsLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadVideos();
        await LoadChannels();
        await LoadPlaylists();
    }

    private async Task LoadVideos()
    {
        _videos = await videoData.GetAllVideosAsync();
        _isVideosLoading = false;
    }

    private async Task LoadChannels()
    {
        _channels = await channelData.GetAllChannelsAsync();
        _isChannelsLoading = false;
    }

    private async Task LoadPlaylists()
    {
        _playlists = await playlistData.GetAllPlaylistsAsync();
        _isPlaylistsLoading = false;
    }

    private async Task DownloadAll()
    {
        if (_videos?.Count <= 0)
        {
            snackbar.Add("No videos are available.");
            return;
        }

        try
        {
            var cancellationToken = tokenHelper.InitializeToken(ref _allVideosTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _videosProgress, value);
            });

            foreach (var v in _videos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentDownloadingVideo = v.Title;

                await youtube.DownloadVideoAsync(
                    v.Url,
                    progressReport,
                    cancellationToken);

                snackbar.Add($"Successfully downloaded {v.Title}");
            }

            _currentDownloadingVideo = "";
            CancelDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"There was an issue downloading your videos: {ex.Message}", Severity.Error);
        }
    }

    private async Task DeleteVideo(VideoModel video)
    {
        var v = _videos.First(v => v.VideoId == video.VideoId || v.Url == video.Url);

        if (v is not null)
        {
            _videos.Remove(v);
            await videoData.DeleteVideoAsync(v);
        }
    }

    private async Task DeleteChannel(ChannelModel channel)
    {
        var c = _channels.First(c => c.ChannelId == channel.ChannelId || c.Url == channel.Url);

        if (c is not null)
        {
            _channels.Remove(c);
            await channelData.DeleteChannelAsync(c);
        }
    }

    private async Task DeletePlaylist(PlaylistModel playlist)
    {
        var p = _playlists.First(p => p.PlaylistId == playlist.PlaylistId || p.Url == playlist.Url);

        if (p is not null)
        {
            _playlists.Remove(p);
            await playlistData.DeletePlaylistAsync(p);
        }
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(_videos, searchInput);
    }

    private async Task<IEnumerable<string>> SearchChannels(string searchInput)
    {
        return await searchHelper.SearchAsync(_channels, searchInput);
    }
    
    private async Task<IEnumerable<string>> SearchPlaylists(string searchInput)
    {
        return await searchHelper.SearchAsync(_playlists, searchInput);
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private void FilterVideos()
    {
        _videos = searchHelper.FilterList(_videos, _videoSearchText);
    }

    private void FilterChannels()
    {
        _channels = searchHelper.FilterList(_channels, _channelSearchText);
    }

    private void FilterPlaylists()
    {
        _playlists = searchHelper.FilterList(_playlists, _playlistSearchText);
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        progressVariable = value;
        StateHasChanged();
    }

    private void CancelDownload()
    {
        tokenHelper.CancelRequest(ref _allVideosTokenSource);
        _videosProgress = 0;
        _currentDownloadingVideo = "";
    }

    private string GetDownloadVideoText()
    {
        if (_videos?.Count <= 0)
        {
            return "Download Video";
        }

        if (_videos?.Count == 1)
        {
            return "Download 1 Video";
        }

        return $"Download {_videos?.Count} Videos";
    }

    private string GetVideoSearchBarText()
    {
        if (_videos?.Count <= 0)
        {
            return "Search Video";
        }

        if (_videos?.Count == 1)
        {
            return "Search 1 Video";
        }

        return $"Search {_videos?.Count} Videos";
    }

    private string GetChannelSearchBarText()
    {
        if (_channels?.Count <= 0)
        {
            return "Search Channel";
        }

        if (_channels?.Count == 1)
        {
            return "Search 1 Channel";
        }

        return $"Search {_channels?.Count} Channels";
    }

    private string GetPlaylistSearchBarText()
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