using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SavedMediaVideo
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private const int ItemsPerPage = 6;
    private SettingsLibrary _settings;
    private CancellationTokenSource _allVideosTokenSource;
    private CancellationTokenSource _tokenSource;
    private List<VideoModel> _videos = new();
    private List<VideoModel> _visibleVideos = new();
    private string _searchText = "";
    private string _currentDownloadingVideo = "";
    private double _videosProgress = 0;
    private int _loadedItems = 6;
    private bool _isVisible = false;

    protected override async Task OnInitializedAsync()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(nameof(SavedMediaVideo), ItemsPerPage);

        _settings = await settingsData.GetSettingsAsync();
        await LoadVideos();
    }

    private void LoadMoreVideos()
    {
        int videosCount = _videos.Count;
        _loadedItems += ItemsPerPage;

        if (_loadedItems > videosCount)
        {
            _loadedItems = videosCount;
        }

        _visibleVideos = _videos.Take(_loadedItems).ToList();
        loadedItemsCache.SetLoadedItemsCount(nameof(SavedMediaVideo), _loadedItems);
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

    private async Task UpdateAllVideos()
    {
        try
        {
            var videosCopy = _videos.ToList();
            var token = tokenHelper.InitializeToken(ref _tokenSource);

            foreach (var v in videosCopy)
            {
                token.ThrowIfCancellationRequested();
                await UpdateVideo(v, token);
            }

            CancelUpdateVideo();
            snackbar.Add("Successfully updated.");
        }
        catch (OperationCanceledException)
        {
            snackbar.Add("Update canceled.", Severity.Error);
        }
        catch 
        {
            snackbar.Add("An error occured while updating.", Severity.Error);
        }
    }

    private async Task UpdateVideo(VideoModel video, CancellationToken token)
    {
        var newVideo = await youtube.GetVideoAsync(video.Url, token);

        if (newVideo is null)
        {
            RemoveVideo(video);
            snackbar.Add($"{video?.Title} no longer exists. It has been deleted", Severity.Error);
            await videoData.DeleteVideoAsync(video);
        }
        else
        {
            await videoData.SetVideoAsync(video.Url, video.VideoId);
        }
    }

    private async Task DeleteAllVideos()
    {
        CloseDialog();

        var videosCopy = _videos?.ToList();

        foreach (var v in videosCopy)
        {
            RemoveVideo(v);
            await videoData.DeleteVideoAsync(v);
        }
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterVideos();
    }

    private void FilterVideos()
    {
        _videos = searchHelper
            .FilterList(_videos, _searchText);

        _visibleVideos = searchHelper
            .FilterList(_videos, _searchText)
            .Take(_loadedItems)
            .ToList();
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        if (Math.Abs(value - progressVariable) < 0.1)
            return;

        progressVariable = value;
        StateHasChanged();
    }

    private void CancelDownload()
    {
        tokenHelper.CancelRequest(ref _allVideosTokenSource);
        _videosProgress = 0;
        _currentDownloadingVideo = "";
    }

    private void CancelUpdateVideo()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
    }

    private void RemoveVideo(VideoModel video)
    {
        _visibleVideos?.Remove(video);
        _videos?.Remove(video);
    }

    private void OpenDialog()
    {
        _isVisible = true;
    }

    private void CloseDialog()
    {
        _isVisible = false;
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
        bool isFFmpegEmpty = string.IsNullOrWhiteSpace(_settings.FfmpegPath) is false;
        bool ffmpPegDoesNotExist = File.Exists(_settings.FfmpegPath) is false;
        if (isFFmpegEmpty && ffmpPegDoesNotExist)
        {
            return true;
        }

        return false;
    }
}