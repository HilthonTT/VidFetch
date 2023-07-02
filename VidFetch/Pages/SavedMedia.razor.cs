using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class SavedMedia
{
    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private CancellationTokenSource _allVideosTokenSource;
    private List<VideoModel> _videos = new();
    private List<VideoModel> _visibleVideos = new();

    private List<ChannelModel> _channels = new();
    private List<ChannelModel> _visibleChannels = new();

    private List<PlaylistModel> _playlists = new();
    private List<PlaylistModel> _visiblePlaylists = new();

    private string _videoSearchText = "";
    private string _channelSearchText = "";
    private string _playlistSearchText = "";
    private string _currentDownloadingVideo = "";
    private double _videosProgress = 0;
    private bool _isVideosLoading = true;
    private bool _isPlaylistsLoading = true;
    private bool _isChannelsLoading = true;

    private int _loadedVideos = 6;
    private int _loadedChannels = 6;
    private int _loadedPlaylists = 6;

    protected override async Task OnInitializedAsync()
    {
        await LoadVideos();
        await LoadChannels();
        await LoadPlaylists();
    }

    private void LoadMoreVideos()
    {
        int itemsPerPage = 6;
        int videosCount = _videos.Count;

        _loadedVideos += itemsPerPage;

        if (_loadedVideos > videosCount)
        {
            _loadedVideos = videosCount;
        }

        _visibleVideos = _videos.Take(_loadedVideos).ToList();
    }

    private void LoadMoreChannels()
    {
        int itemsPerPage = 6;
        int channelCount = _channels.Count;

        _loadedChannels += itemsPerPage;

        if (_loadedChannels > channelCount)
        {
            _loadedChannels = channelCount;
        }

        _visibleChannels = _channels.Take(_loadedChannels).ToList();
    }

    private void LoadMorePlaylists()
    {
        int itemsPerPage = 6;
        int playlistCount = _playlists.Count;

        _loadedPlaylists += itemsPerPage;

        if (_loadedPlaylists > playlistCount)
        {
            _loadedPlaylists = playlistCount;
        }

        _visiblePlaylists = _playlists.Take(_loadedPlaylists).ToList();
    }

    private async Task LoadVideos()
    {
        _videos = await videoData.GetAllVideosAsync();
        _visibleVideos = _videos.Take(_loadedVideos).ToList();
        _isVideosLoading = false;
    }

    private async Task LoadChannels()
    {
        _channels = await channelData.GetAllChannelsAsync();
        _visibleChannels = _channels.Take(_loadedChannels).ToList();
        _isChannelsLoading = false;
    }

    private async Task LoadPlaylists()
    {
        _playlists = await playlistData.GetAllPlaylistsAsync();
        _visiblePlaylists = _playlists.Take(_loadedPlaylists).ToList();
        _isPlaylistsLoading = false;
    }

    private async Task DownloadSavedVideos()
    {
        if (_videos?.Count <= 0)
        {
            snackbar.Add("No videos are available.");
            return;
        }

        try
        {
            if (IsFFmpegPathInvalid())
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var token = tokenHelper.InitializeToken(ref _allVideosTokenSource);

            var progress = new Progress<double>(value =>
            {
                UpdateProgress(ref _videosProgress, value);
            });

            foreach (var v in _videos)
            {
                await DownloadVideo(v, progress, token);
            }

            _currentDownloadingVideo = "";
            CancelDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"There was an issue downloading your videos: {ex.Message}", Severity.Error);
        }
    }

    private async Task DownloadVideo(VideoModel video, Progress<double> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _currentDownloadingVideo = video.Title;

        await youtube.DownloadVideoAsync(video.Url, progress, token);

        snackbar.Add($"Successfully downloaded {video.Title}");
    }

    private async Task DeleteVideo(VideoModel video)
    {
        _videos.Remove(video);
        _visibleVideos.Remove(video);
        await videoData.DeleteVideoAsync(video);
    }

    private async Task DeleteChannel(ChannelModel channel)
    {
        _channels.Remove(channel);
        _visibleChannels.Remove(channel);
        await channelData.DeleteChannelAsync(channel);
    }

    private async Task DeletePlaylist(PlaylistModel playlist)
    {
        _playlists.Remove(playlist);
        _visiblePlaylists.Remove(playlist);
        await playlistData.DeletePlaylistAsync(playlist);
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
        _videos = searchHelper
            .FilterList(_videos, _videoSearchText);

        _visibleVideos = searchHelper
            .FilterList(_videos, _videoSearchText)
            .Take(_loadedVideos)
            .ToList();
    }

    private void FilterChannels()
    {
        _channels = searchHelper
            .FilterList(_channels, _channelSearchText);

        _visibleChannels = searchHelper
            .FilterList(_channels, _channelSearchText)
            .Take(_loadedChannels)
            .ToList();
    }

    private void FilterPlaylists()
    {
        _playlists = searchHelper
            .FilterList(_playlists, _playlistSearchText);

        _visiblePlaylists = searchHelper
            .FilterList(_playlists, _playlistSearchText)
            .Take(_loadedPlaylists)
            .ToList();
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

    private bool IsFFmpegPathInvalid()
    {
        string path = settingsLibrary.FfmpegPath;
        bool isPathNotEmptyOrNull = path is not null;
        bool FileExists = File.Exists(path);

        if (isPathNotEmptyOrNull && FileExists)
        {
            return true;
        }

        return false;
    }
}