using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class SavedMedia
{
    private CancellationTokenSource allVideosTokenSource;
    private List<VideoModel> videos = new();
    private string searchText = "";
    private string errorMessage = "";
    private string currentDownloadingVideo = "";
    private double videosProgress = 0;
    private bool isVideosLoading = true;
    protected override async Task OnInitializedAsync()
    {
        await LoadVideos();
    }

    private async Task LoadVideos()
    {
        videos = await videoData.GetAllVideosAsync();
        isVideosLoading = false;
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
                    settingsLibrary.SelectedPath,
                    settingsLibrary.SelectedFormat,
                    progressReport,
                    cancellationToken,
                    settingsLibrary.DownloadSubtitles);

                AddSnackbar(v.Title);
            }

            currentDownloadingVideo = "";
            CancelDownload();
        }
        catch (Exception ex)
        {
            errorMessage = $"There was an issue downloading your videos: {ex.Message}";
        }
    }

    private async Task DeleteVideo(VideoModel video)
    {
        var v = videos.FirstOrDefault(v => v.VideoId == video.VideoId || v.Url == video.Url);

        videos.Remove(v);
        await videoData.DeleteVideoAsync(v);
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videos, searchInput);
    }

    private async Task OpenFileLocation()
    {
        if (string.IsNullOrWhiteSpace(settingsLibrary.SelectedPath))
        {
            return;
        }

        await folderHelper.OpenFolderLocationAsync(settingsLibrary.SelectedPath);
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