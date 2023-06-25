using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Index
{
    private CancellationTokenSource _playlistTokenSource;
    private CancellationTokenSource _allVideosTokenSource;
    private CancellationTokenSource _videoTokenSource;
    private string _errorMessage = "";
    private string _videoUrl = "";
    private string _channelUrl = "";
    private string _playlistUrl = "";
    private string _firstVideoInPlaylistUrl = "";

    private string _videoSearchText = "";
    private string _channelSearchText = "";
    private string _playlistSearchText = "";
    private string _playlistVideoSearchText = "";

    private string _currentDownloadingVideo = "";
    private string _currentDownloadingPlaylistVideo = "";
    
    private double _videosProgress = 0;
    private double _playlistProgress = 0;
    private double _firstPlaylistProgress = 0;

    private bool _showDialog = false;
    private bool _isVideoLoading = false;
    private bool _isPlaylistLoading = false;
    private bool _isChannelLoading = false;

    private async Task LoadVideoOrPlaylistVideos()
    {
        try
        {
            _errorMessage = "";
            if (string.IsNullOrWhiteSpace(_videoUrl))
            {
                return;
            }

            if (Uri.IsWellFormedUriString(_videoUrl, UriKind.Absolute))
            {
                if (IsPlaylistUrl())
                {
                    await LoadPlaylistVideos();
                    _showDialog = true;
                    _firstVideoInPlaylistUrl = _videoUrl;
                }
                else
                {
                    await LoadSingleVideo();
                }
            }
            else
            {
                _errorMessage = "Not a valid Url.";
            }

            _videoUrl = "";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }

    private async Task LoadChannel()
    {
        try
        {
            _errorMessage = "";

            if (string.IsNullOrWhiteSpace(_channelUrl))
            {
                return;
            }

            if (Uri.IsWellFormedUriString(_channelUrl, UriKind.Absolute))
            {
                _isChannelLoading = true;

                var channel = await youtube.GetChannelAsync(_channelUrl);
                bool channelIsNull = videoLibrary.Channels.FirstOrDefault(c => c.ChannelId == channel.ChannelId) is null;

                if (channelIsNull)
                {
                    videoLibrary.Channels.Add(channel);
                }
                _isChannelLoading = false;
            }
            else
            {
                _errorMessage = "Not a valid Url.";
            }

            _channelUrl = "";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _isChannelLoading = false;
        }
    }

    private async Task LoadPlaylist()
    {
        try
        {
            _errorMessage = "";

            if (string.IsNullOrWhiteSpace(_playlistUrl))
            {
                return;
            }

            if (Uri.IsWellFormedUriString(_playlistUrl, UriKind.Absolute))
            {
                _isPlaylistLoading = true;

                var playlist = await youtube.GetPlaylistAsync(_playlistUrl);
                bool playlistIsNull = videoLibrary.Playlists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId) is null;

                if (playlistIsNull)
                {
                    videoLibrary.Playlists.Add(playlist);
                }
                _isPlaylistLoading = false;
            }
            else
            {
                _errorMessage = "Not a valid Url.";
            }

            _playlistUrl = "";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            _isPlaylistLoading = false;
        }
    }

    private async Task LoadPlaylistVideos()
    {
        _isPlaylistLoading = true;
        var videos = await youtube.GetPlayListVideosAsync(_videoUrl);

        foreach (var v in videos)
        {
            if (IsVideoNotLoaded(v.VideoId))
            {
                videoLibrary.PlaylistVideos.Add(v);
            }
        }

        if (settingsLibrary.SaveVideos)
        {
            await SavePlaylistVideos();
        }

        _isPlaylistLoading = false;
    }

    private async Task LoadSingleVideo()
    {
        _isVideoLoading = true;
        var video = await youtube.GetVideoAsync(_videoUrl);

        if (IsVideoNotLoaded(video.VideoId))
        {
            videoLibrary.Videos.Add(video);
        }

        if (settingsLibrary.SaveVideos)
        {
            await SaveVideos();
        }

        _isVideoLoading = false;
    }

    private async Task SaveVideos()
    {
        foreach (var v in videoLibrary.Videos)
        {
            await videoData.SetVideoAsync(v.Url, v.VideoId);
        }
    }

    private async Task SavePlaylistVideos()
    {
        foreach(var v in videoLibrary.PlaylistVideos)
        {
            await videoData.SetVideoAsync(v.Url, v.VideoId);
        }
    }

    private async Task DownloadVideo(string url)
    {
        try
        {
            _errorMessage = "";
            var cancellationToken = tokenHelper.InitializeToken(ref _videoTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _firstPlaylistProgress, value);
            });

            await youtube.DownloadVideoAsync(
                url,
                progressReport,
                cancellationToken);

            CancelVideoDownload();
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }

    private async Task DownloadAllVideos()
    {
        try
        {
            _errorMessage = "";
            var cancellationToken = tokenHelper.InitializeToken(ref _allVideosTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _videosProgress, value);
            });

            foreach (var v in videoLibrary.Videos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentDownloadingVideo = v.Title;

                await youtube.DownloadVideoAsync(
                    v.Url,
                    progressReport,
                    cancellationToken);

                AddSnackbar(v.Title);
            }

            _currentDownloadingVideo = "";
            CancelVideosDownload();
        }
        catch (Exception ex)
        {
            _errorMessage = $"There was an issue while downloading your videos: {ex.Message}";
        }
    }

    private async Task DownloadAllPlaylists()
    {
        try
        {
            _errorMessage = "";
            var cancellationToken = tokenHelper.InitializeToken(ref _playlistTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _playlistProgress, value);
            });

            foreach (var v in videoLibrary.PlaylistVideos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentDownloadingPlaylistVideo = v.Title;

                await youtube.DownloadVideoAsync(
                    v.Url,
                    progressReport,
                    cancellationToken);

                AddSnackbar(v.Title);
            }

            _currentDownloadingPlaylistVideo = "";
            CancelPlaylistDownload();
        }
        catch (Exception ex)
        {
            _errorMessage = $"There was an issue while downloading your playlist: {ex.Message}";
        }
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Videos, searchInput);
    }

    private async Task<IEnumerable<string>> SearchPlaylistVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.PlaylistVideos, searchInput);
    }

    private async Task<IEnumerable<string>> SearchChannels(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Channels, searchInput);
    }

    private async Task<IEnumerable<string>> SearchPlaylists(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Playlists, searchInput);
    }

    private void FilterVideos()
    {
        videoLibrary.Videos = searchHelper.FilterList(videoLibrary.Videos, _videoSearchText);
    }

    private void FilterChannels()
    {
        videoLibrary.Channels = searchHelper.FilterList(videoLibrary.Channels, _channelSearchText);
    }

    private void FilterPlaylists()
    {
        videoLibrary.Playlists = searchHelper.FilterList(videoLibrary.Playlists, _playlistSearchText);
    }

    private void FilterPlaylistVideo()
    {
        videoLibrary.PlaylistVideos = searchHelper.FilterList(videoLibrary.PlaylistVideos, _playlistVideoSearchText);
    }

    private void AddSnackbar(string title)
    {
        snackbar.Add($"Successfully downloaded {title}", Severity.Normal);
    }

    private void CancelVideosDownload()
    {
        tokenHelper.CancelRequest(ref _allVideosTokenSource);
        _videosProgress = 0;
        _currentDownloadingVideo = "";
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref _videoTokenSource);
    }

    private void CancelPlaylistDownload()
    {
        tokenHelper.CancelRequest(ref _playlistTokenSource);
        _playlistProgress = 0;
        _currentDownloadingPlaylistVideo = "";
    }

    private void ClearVideos()
    {
        videoLibraryHelper.ClearVideos(ref _videosProgress);
    }

    private void ClearPlaylistVideos()
    {
        videoLibraryHelper.ClearPlaylistVideos(ref _playlistProgress);
    }

    private void ClearChannels()
    {
        videoLibraryHelper.ClearChannels();
    }

    private void ClearPlaylists()
    {
        videoLibraryHelper.ClearPlaylists();
    }

    private void RemoveVideo(VideoModel video)
    {
        videoLibraryHelper.RemoveVideo(video);
    }

    private void RemovePlaylistVideo(VideoModel video)
    {
        var v = videoLibrary.PlaylistVideos.FirstOrDefault(
            v => v.VideoId == video.VideoId || v.Url == video.Url
        );

        videoLibraryHelper.RemovePlaylistVideo(v);
    }

    private void RemoveChannel(ChannelModel channel)
    {
        var c = videoLibrary.Channels.FirstOrDefault(
            c => c.ChannelId == channel.ChannelId || c.Url == channel.Url
        );

        videoLibraryHelper.RemoveChannel(c);
    }

    private void RemovePlaylist(PlaylistModel playlist)
    {
        var p = videoLibrary.Playlists.FirstOrDefault(
            p => p.PlaylistId == playlist.PlaylistId || p.Url == playlist.Url
        );

        videoLibraryHelper.RemovePlaylist(p);
    }

    private void ToggleDialog()
    {
        _showDialog = !_showDialog;
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        progressVariable = value;
        StateHasChanged();
    }

    private async Task OpenFileLocation()
    {
        if (string.IsNullOrWhiteSpace(settingsLibrary.SelectedPath))
        {
            return;
        }

        await folderHelper.OpenFolderLocationAsync();
    }

    private string GetDownloadVideoText()
    {
        if (videoLibrary.Videos?.Count <= 0)
        {
            return "Download Video";
        }

        if (videoLibrary.Videos?.Count == 1)
        {
            return "Download 1 Video";
        }

        return $"Download {videoLibrary.Videos?.Count} Videos";
    }

    private string GetVideoSearchBarText()
    {
        if (videoLibrary?.Videos.Count <= 0)
        {
            return "Search Video";
        }

        if (videoLibrary.Videos?.Count == 1)
        {
            return "Search 1 Video";
        }

        return $"Search {videoLibrary.Videos?.Count} Videos";
    }

    private string GetChannelSearchBarText()
    {
        if (videoLibrary?.Channels.Count <= 0)
        {
            return "Search Channel";
        }

        if (videoLibrary.Channels?.Count == 1)
        {
            return "Search 1 Channel";
        }

        return $"Search {videoLibrary.Channels?.Count} Channels";
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

    private string GetPlaylistVideoSearchBarText()
    {
        if (videoLibrary?.PlaylistVideos.Count <= 0)
        {
            return "Search Playlist Video";
        }

        if (videoLibrary.PlaylistVideos?.Count == 1)
        {
            return "Search 1 Playlist Video";
        }

        return $"Search {videoLibrary.PlaylistVideos?.Count} Playlist Videos";
    }

    private bool IsVideoNotLoaded(string videoId)
    {
        return videoLibrary.Videos.Any(v => v.VideoId == videoId) is false;
    }

    private bool IsPlaylistUrl()
    {
        return _videoUrl.Contains("list=");
    }

    private int GetIndex(VideoModel playlistVideo)
    {
        int index = videoLibrary.PlaylistVideos.IndexOf(playlistVideo);
        return index + 1;
    }
}