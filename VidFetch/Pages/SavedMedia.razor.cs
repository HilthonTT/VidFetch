using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class SavedMedia
{
    private CancellationTokenSource _allVideosTokenSource;
    private List<VideoModel> _videos = new();
    private string _searchText = "";
    private string _errorMessage = "";
    private string _currentDownloadingVideo = "";
    private double _videosProgress = 0;
    private bool _isVideosLoading = true;
    protected override async Task OnInitializedAsync()
    {
        await LoadVideos();
    }

    private async Task LoadVideos()
    {
        _videos = await videoData.GetAllVideosAsync();
        _isVideosLoading = false;
    }

    private async Task DownloadAll()
    {
        if (_videos?.Count <= 0)
        {
            _errorMessage = "No videos are available.";
            return;
        }

        try
        {
            _errorMessage = "";
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
                    settingsLibrary.SelectedPath,
                    settingsLibrary.SelectedFormat,
                    progressReport,
                    cancellationToken,
                    settingsLibrary.DownloadSubtitles);

                AddSnackbar(v.Title);
            }

            _currentDownloadingVideo = "";
            CancelDownload();
        }
        catch (Exception ex)
        {
            _errorMessage = $"There was an issue downloading your videos: {ex.Message}";
        }
    }

    private async Task DeleteVideo(VideoModel video)
    {
        var v = _videos.FirstOrDefault(v => v.VideoId == video.VideoId || v.Url == video.Url);

        _videos.Remove(v);
        await videoData.DeleteVideoAsync(v);
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(_videos, searchInput);
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
        _videos = searchHelper.FilterList(_videos, _searchText);
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
}