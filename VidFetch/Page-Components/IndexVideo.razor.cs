using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexVideo
{
    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private CancellationTokenSource _playlistTokenSource;
    private CancellationTokenSource _allVideosTokenSource;
    private CancellationTokenSource _videoTokenSource;
    private string _videoUrl = "";
    private string _videoSearchText = "";
    private string _playlistVideoSearchText = "";
    private string _currentDownloadingVideo = "";
    private string _currentDownloadingPlaylistVideo = "";
    private string _firstVideoInPlaylistUrl = "";
    private bool _showDialog = false;
    private bool _isVideoLoading = false;
    private bool _isPlaylistLoading = false;
    private double _videosProgress = 0;
    private double _playlistProgress = 0;
    private double _firstPlaylistProgress = 0;
    private async Task LoadVideoOrPlaylistVideos()
    {
        try
        {
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
                snackbar.Add($"Invalid Url", Severity.Warning);
            }

            _videoUrl = "";
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
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
        foreach (var v in videoLibrary.PlaylistVideos)
        {
            await videoData.SetVideoAsync(v.Url, v.VideoId);
        }
    }

    private async Task DownloadAllVideos()
    {
        try
        {
            if (File.Exists(settingsLibrary.FfmpegPath) is false)
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var cancellationToken = tokenHelper.InitializeToken(ref _allVideosTokenSource);
            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _videosProgress, value);
            });
            foreach (var v in videoLibrary.Videos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentDownloadingVideo = v.Title;
                await youtube.DownloadVideoAsync(v.Url, progressReport, cancellationToken);
                AddSnackbar(v.Title);
            }

            _currentDownloadingVideo = "";
            CancelVideosDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task DownloadVideo(string url)
    {
        try
        {
            if (File.Exists(settingsLibrary.FfmpegPath)is false)
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var cancellationToken = tokenHelper.InitializeToken(ref _videoTokenSource);
            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _firstPlaylistProgress, value);
            });
            await youtube.DownloadVideoAsync(url, progressReport, cancellationToken);
            CancelVideoDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task DownloadAllPlaylists()
    {
        try
        {
            if (File.Exists(settingsLibrary.FfmpegPath)is false)
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var cancellationToken = tokenHelper.InitializeToken(ref _playlistTokenSource);
            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _playlistProgress, value);
            });
            foreach (var v in videoLibrary.PlaylistVideos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _currentDownloadingPlaylistVideo = v.Title;
                await youtube.DownloadVideoAsync(v.Url, progressReport, cancellationToken);
                AddSnackbar(v.Title);
            }

            _currentDownloadingPlaylistVideo = "";
            CancelPlaylistDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
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

    private void FilterVideos()
    {
        videoLibrary.Videos = searchHelper.FilterList(videoLibrary.Videos, _videoSearchText);
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

    private void CancelPlaylistDownload()
    {
        tokenHelper.CancelRequest(ref _playlistTokenSource);
        _playlistProgress = 0;
        _currentDownloadingPlaylistVideo = "";
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref _videoTokenSource);
    }

    private void ClearVideos()
    {
        videoLibraryHelper.ClearVideos(ref _videosProgress);
    }

    private void ClearPlaylistVideos()
    {
        videoLibraryHelper.ClearPlaylistVideos(ref _playlistProgress);
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
        return videoLibrary.Videos.Any(v => v.VideoId == videoId)is false;
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