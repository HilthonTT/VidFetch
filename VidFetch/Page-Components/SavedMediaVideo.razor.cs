using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SavedMediaVideo
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private CancellationTokenSource _allVideosTokenSource;
    private List<VideoModel> _videos = new();
    private List<VideoModel> _visibleVideos = new();
    private string _searchText = "";
    private string _currentDownloadingVideo = "";
    private double _videosProgress = 0;
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        await LoadVideos();
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

    private async Task LoadVideos()
    {
        await OpenLoading.InvokeAsync(true);
        _videos = await videoData.GetAllVideosAsync();
        _visibleVideos = _videos.Take(_loadedItems).ToList();
        await OpenLoading.InvokeAsync(false);
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
            if (IsFFmpegInvalid())
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

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(_videos, searchInput);
    }

    private async Task DeleteVideo(VideoModel video)
    {
        _videos.Remove(video);
        _visibleVideos.Remove(video);
        await videoData.DeleteVideoAsync(video);
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private void FilterVideos()
    {
        _videos = searchHelper.FilterList(_videos, _searchText);
        _visibleVideos = searchHelper.FilterList(_videos, _searchText).Take(_loadedItems).ToList();
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

    private string GetSearchBarText()
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

    private bool IsFFmpegInvalid()
    {
        bool isFFmpegEmpty = string.IsNullOrWhiteSpace(settingsLibrary.FfmpegPath)is false;
        bool ffmpPegDoesNotExist = File.Exists(settingsLibrary.FfmpegPath)is false;
        if (isFFmpegEmpty && ffmpPegDoesNotExist)
        {
            return true;
        }

        return false;
    }
}