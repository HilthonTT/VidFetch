using MudBlazor;
using YoutubeExplode.Videos;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class SavedMedia
{
    private const string DefaultDownloadPath = "Download Folder";
    private const string DefaultExtension = ".mp4";
    private CancellationTokenSource allVideosTokenSource;
    private List<VideoModel> videos = new();
    private List<string> downloadPaths = new();
    private List<string> videoExtensions = new();
    private string selectedPath = DefaultDownloadPath;
    private string selectedExtension = DefaultExtension;
    private string searchText = "";
    private string errorMessage = "";
    private string currentDownloadingVideo = "";
    private double videosProgress = 0;
    private bool isVideosLoading = true;
    protected override async Task OnInitializedAsync()
    {
        LoadPathsAndExtensions();
        await LoadVideos();
        await LoadStates();
    }

    private void LoadPathsAndExtensions()
    {
        downloadPaths = defaultData.GetDownloadPaths();
        videoExtensions = defaultData.GetVideoExtensions();
    }

    private async Task LoadVideos()
    {
        videos = await videoData.GetAllVideosAsync();
        isVideosLoading = false;
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

    private async Task DownloadAll()
    {
        if (videos?.Count <= 0)
        {
            errorMessage = "No videos are available.";
            return;
        }

        try
        {
            errorMessage = "";
            var cancellationToken = tokenHelper.InitializeToken(ref allVideosTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref videosProgress, value);
            });

            foreach (var v in videos)
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
            CancelDownload();
            await SaveStates();
        }
        catch (Exception ex)
        {
            errorMessage = $"There was an issue downloading your videos: {ex.Message}";
        }
    }

    private async Task DeleteVideo(Video video)
    {
        var v = videos.FirstOrDefault(v => v.VideoId == video.Id || v.Url == video.Url);

        videos.Remove(v);
        await videoData.DeleteAsync(v);
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videos, searchInput);
    }

    private async Task OnButtonClick(string path)
    {
        selectedPath = path;
        await SaveStates();
    }

    private async Task OpenFileLocation()
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        await folderHelper.OpenFolderLocationAsync(selectedPath);
    }

    private void FilterVideos()
    {
        videos = searchHelper.FilterList(videos, searchText);
    }

    private void AddSnackbar(string title)
    {
        snackbar.Add($"Successfully downloaded {title}", Severity.Normal);
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        progressVariable = value;
        StateHasChanged();
    }

    private void CancelDownload()
    {
        tokenHelper.CancelRequest(ref allVideosTokenSource);
        videosProgress = 0;
        currentDownloadingVideo = "";
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
        if (videos?.Count <= 0)
        {
            return "Download Video";
        }

        if (videos?.Count == 1)
        {
            return "Download 1 Video";
        }

        return $"Download {videos?.Count} Videos";
    }

    private string GetVideoSearchBarText()
    {
        if (videos?.Count <= 0)
        {
            return "Search Video";
        }

        if (videos?.Count == 1)
        {
            return "Search 1 Video";
        }

        return $"Search {videos?.Count} Videos";
    }
}