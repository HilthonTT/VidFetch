using MudBlazor;
using VidFetchLibrary.Models;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetch.Pages;

public partial class Index
{
    private const string DefaultDownloadPath = "Download Folder";
    private const string DefaultExtension = ".mp4";
    private CancellationTokenSource playlistTokenSource;
    private CancellationTokenSource allVideosTokenSource;
    private CancellationTokenSource videoTokenSource;
    private List<string> downloadPaths = new();
    private List<string> videoExtensions = new();
    private string selectedPath = DefaultDownloadPath;
    private string selectedExtension = DefaultExtension;
    private string youtubeUrl = "";
    private string errorMessage = "";
    private string videoSearchText = "";
    private string playlistVideoSearchText = "";
    private string currentDownloadingVideo = "";
    private string currentDownloadingPlaylistVideo = "";
    private string playlistUrl = "";
    private double videosProgress = 0;
    private double playlistProgress = 0;
    private double firstPlaylistProgress = 0;
    private bool showDialog = false;
    private bool isVideoLoading = false;
    private bool isPlaylistLoading = false;

    protected override async Task OnInitializedAsync()
    {
        LoadPathsAndExtensions();
        await LoadStates();
    }

    private void LoadPathsAndExtensions()
    {
        downloadPaths = defaultData.GetDownloadPaths();
        videoExtensions = defaultData.GetVideoExtensions();
    }

    private async Task LoadStates()
    {
        selectedPath = await secureStorage.GetAsync(nameof(selectedPath)) ?? DefaultDownloadPath;
        selectedExtension = await secureStorage.GetAsync(nameof(selectedExtension)) ?? DefaultExtension;
    }

    private async Task SaveStates()
    {
        await secureStorage.SetAsync(nameof(selectedPath), selectedPath);
        await secureStorage.SetAsync(nameof(selectedExtension), selectedExtension);
    }

    private async Task LoadVideoOrPlaylist()
    {
        try
        {
            errorMessage = "";
            if (string.IsNullOrWhiteSpace(youtubeUrl))
            {
                return;
            }

            if (IsPlaylistUrl())
            {
                await LoadPlaylistVideos();
                showDialog = true;
                playlistUrl = youtubeUrl;
            }
            else
            {
                await LoadSingleVideo();
            }

            youtubeUrl = "";
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task LoadPlaylistVideos()
    {
        isPlaylistLoading = true;
        var videos = await youtube.GetPlayListVideosAsync(youtubeUrl);

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

        isPlaylistLoading = false;
    }

    private async Task LoadSingleVideo()
    {
        isVideoLoading = true;
        var video = await youtube.GetVideoAsync(youtubeUrl);

        if (IsVideoNotLoaded(video.VideoId))
        {
            videoLibrary.Videos.Add(video);
        }

        if (settingsLibrary.SaveVideos)
        {
            await SaveVideos();
        }

        isVideoLoading = false;
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
            errorMessage = "";
            var cancellationToken = tokenHelper.InitializeToken(ref videoTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref firstPlaylistProgress, value);
            });

            await youtube.DownloadVideoAsync(
                url,
                selectedPath,
                selectedExtension,
                progressReport,
                cancellationToken,
                settingsLibrary.DownloadSubtitles);

            CancelVideoDownload();
            await SaveStates();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task DownloadAllVideos()
    {
        try
        {
            errorMessage = "";
            var cancellationToken = tokenHelper.InitializeToken(ref allVideosTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref videosProgress, value);
            });

            foreach (var v in videoLibrary.Videos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                currentDownloadingVideo = v.Title;

                await youtube.DownloadVideoAsync(
                    v.Url,
                    selectedPath,
                    selectedExtension,
                    progressReport,
                    cancellationToken,
                    settingsLibrary.DownloadSubtitles);

                AddSnackbar(v.Title);
            }

            currentDownloadingVideo = "";
            CancelVideosDownload();
            await SaveStates();
        }
        catch (Exception ex)
        {
            errorMessage = $"There was an issue while downloading your videos: {ex.Message}";
        }
    }

    private async Task DownloadAllPlaylists()
    {
        try
        {
            errorMessage = "";
            var cancellationToken = tokenHelper.InitializeToken(ref playlistTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref playlistProgress, value);
            });

            foreach (var v in videoLibrary.PlaylistVideos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                currentDownloadingPlaylistVideo = v.Title;

                await youtube.DownloadVideoAsync(
                    v.Url,
                    selectedPath,
                    selectedExtension,
                    progressReport,
                    cancellationToken,
                    settingsLibrary.DownloadSubtitles);

                AddSnackbar(v.Title);
            }

            currentDownloadingPlaylistVideo = "";
            CancelPlaylistDownload();
            await SaveStates();
        }
        catch (Exception ex)
        {
            errorMessage = $"There was an issue while downloading your playlist: {ex.Message}";
        }
    }

    private async Task<IEnumerable<string>> SearchPlaylistVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.PlaylistVideos, searchInput);
    }

    private void FilterPlaylistVideo()
    {
        videoLibrary.PlaylistVideos = searchHelper.FilterList(videoLibrary.PlaylistVideos, playlistVideoSearchText);
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Videos, searchInput);
    }

    private void FilterVideos()
    {
        videoLibrary.Videos = searchHelper.FilterList(videoLibrary.Videos, videoSearchText);
    }

    private void AddSnackbar(string title)
    {
        snackbar.Add($"Successfully downloaded {title}", Severity.Normal);
    }

    private void CancelVideosDownload()
    {
        tokenHelper.CancelRequest(ref allVideosTokenSource);
        videosProgress = 0;
        currentDownloadingVideo = "";
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref videoTokenSource);
    }

    private void CancelPlaylistDownload()
    {
        tokenHelper.CancelRequest(ref playlistTokenSource);
        playlistProgress = 0;
        currentDownloadingPlaylistVideo = "";
    }

    private void ClearVideos()
    {
        videoLibraryHelper.ClearVideos(ref videosProgress);
    }

    private void ClearPlaylist()
    {
        videoLibraryHelper.ClearPlaylist(ref playlistProgress);
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
        showDialog = !showDialog;
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        progressVariable = value;
        StateHasChanged();
    }

    private async Task OpenFileLocation()
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        await folderHelper.OpenFolderLocationAsync(selectedPath);
    }

    private async Task OnButtonClick(string path)
    {
        selectedPath = path;
        await SaveStates();
    }

    private string GetButtonClass(string path)
    {
        if (selectedPath == path)
        {
            return "text-success";
        }

        return "text-danger";
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
        return youtubeUrl.Contains("list=");
    }

    private int GetIndex(VideoModel playlistVideo)
    {
        int index = videoLibrary.PlaylistVideos.IndexOf(playlistVideo);
        return index + 1;
    }
}