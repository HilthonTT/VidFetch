using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Index
{
    private CancellationTokenSource _playlistTokenSource;
    private CancellationTokenSource _allVideosTokenSource;
    private CancellationTokenSource _videoTokenSource;
    private string _youtubeUrl = "";
    private string _errorMessage = "";
    private string _videoSearchText = "";
    private string _playlistVideoSearchText = "";
    private string _currentDownloadingVideo = "";
    private string _currentDownloadingPlaylistVideo = "";
    private string _playlistUrl = "";
    private double _videosProgress = 0;
    private double _playlistProgress = 0;
    private double _firstPlaylistProgress = 0;
    private bool _showDialog = false;
    private bool _isVideoLoading = false;
    private bool _isPlaylistLoading = false;

    private async Task LoadVideoOrPlaylist()
    {
        try
        {
            _errorMessage = "";
            if (string.IsNullOrWhiteSpace(_youtubeUrl))
            {
                return;
            }

            if (IsPlaylistUrl())
            {
                await LoadPlaylistVideos();
                _showDialog = true;
                _playlistUrl = _youtubeUrl;
            }
            else
            {
                await LoadSingleVideo();
            }

            _youtubeUrl = "";
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }

    private async Task LoadPlaylistVideos()
    {
        _isPlaylistLoading = true;
        var videos = await youtube.GetPlayListVideosAsync(_youtubeUrl);

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
        var video = await youtube.GetVideoAsync(_youtubeUrl);

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
                settingsLibrary.SelectedPath,
                settingsLibrary.SelectedFormat,
                progressReport,
                cancellationToken,
                settingsLibrary.DownloadSubtitles);

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
                    settingsLibrary.SelectedPath,
                    settingsLibrary.SelectedFormat,
                    progressReport,
                    cancellationToken,
                    settingsLibrary.DownloadSubtitles);

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
                    settingsLibrary.SelectedPath,
                    settingsLibrary.SelectedFormat,
                    progressReport,
                    cancellationToken,
                    settingsLibrary.DownloadSubtitles);

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

    private async Task<IEnumerable<string>> SearchPlaylistVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.PlaylistVideos, searchInput);
    }

    private void FilterPlaylistVideo()
    {
        videoLibrary.PlaylistVideos = searchHelper.FilterList(videoLibrary.PlaylistVideos, _playlistVideoSearchText);
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Videos, searchInput);
    }

    private void FilterVideos()
    {
        videoLibrary.Videos = searchHelper.FilterList(videoLibrary.Videos, _videoSearchText);
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

    private void ClearPlaylist()
    {
        videoLibraryHelper.ClearPlaylist(ref _playlistProgress);
    }

    private void RemoveVideo(VideoModel video)
    {
        videoLibraryHelper.RemoveVideo(video);
    }

    private void RemovePlaylistVideo(VideoModel video)
    {
        var v = videoLibrary.PlaylistVideos.FirstOrDefault(v => v.VideoId == video.VideoId || v.Url == video.Url);
        videoLibraryHelper.RemovePlaylistVideo(v);
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

        await folderHelper.OpenFolderLocationAsync(settingsLibrary.SelectedPath);
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
        return _youtubeUrl.Contains("list=");
    }

    private int GetIndex(VideoModel playlistVideo)
    {
        int index = videoLibrary.PlaylistVideos.IndexOf(playlistVideo);
        return index + 1;
    }
}