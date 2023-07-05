using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexVideo
{
    [Parameter]
    [EditorRequired]
    public EventCallback SwitchEvent { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string> GetPlaylistUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string> AddVideos { get; set; }


    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
    private const string PageName = nameof(IndexVideo);
    private const int ItemsPerPage = 6;

    private SettingsLibrary _settings;
    private CancellationTokenSource _allVideosTokenSource;
    private List<VideoModel> _visibleVideos = new();
    private string _videoUrl = "";
    private string _searchText = "";
    private string _currentDownloadingVideo = "";
    private double _videosProgress = 0;
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);

        _visibleVideos = videoLibrary.Videos.Take(_loadedItems).ToList();

        _settings = await settingsData.GetSettingsAsync();
    }

    private void LoadMoreVideos()
    {
        int videosCount = videoLibrary.Videos.Count;

        _loadedItems += ItemsPerPage;
 
        if (_loadedItems > videosCount)
        {
            _loadedItems = videosCount;
        }

        _visibleVideos = videoLibrary.Videos.
            Take(_loadedItems)
            .ToList();

        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
    }

    private async Task LoadVideoOrPlaylistVideos()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_videoUrl))
            {
                return;
            }

            if (IsPlaylistUrl())
            {
                await GetPlaylistUrl.InvokeAsync(_videoUrl);
                await AddVideos.InvokeAsync(_videoUrl);

                await SwitchEvent.InvokeAsync();
            }
            else
            {
                await LoadSingleVideo();
                _visibleVideos = videoLibrary.Videos.Take(_loadedItems).ToList();
            }

            _videoUrl = "";
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task LoadSingleVideo()
    {
        await OpenLoading.InvokeAsync(true);
        var video = await youtube.GetVideoAsync(_videoUrl);

        if (IsVideoNotLoaded(video.VideoId))
        {
            videoLibrary.Videos.Add(video);
        }

        if (_settings.SaveVideos)
        {
            await SaveVideos();
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task SaveVideos()
    {
        foreach (var v in videoLibrary.Videos)
        {
            await videoData.SetVideoAsync(v.Url, v.VideoId);
        }
    }

    private async Task DownloadAllVideos()
    {
        try
        {
            if (IsFFmpegInvalid())
            {
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
            }

            var token = tokenHelper.InitializeToken(ref _allVideosTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _videosProgress, value);
            });

            var videosCopy = _visibleVideos.ToList();

            foreach (var v in videosCopy)
            {
                await DownloadVideo(v, progressReport, token);
            }

            _currentDownloadingVideo = "";
            CancelVideosDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }

    private async Task DownloadVideo(VideoModel video, Progress<double> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _currentDownloadingVideo = video.Title;

        await youtube.DownloadVideoAsync(video.Url, progress, token);

        if (_settings.RemoveAfterDownload)
        {
            videoLibrary.Videos.Remove(video);
            _visibleVideos.Remove(video);
        }

        AddSnackbar(video.Title);
    }

    private async Task<IEnumerable<string>> SearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.Videos, searchInput);
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterVideos();
    }

    private void FilterVideos()
    {
        videoLibrary.Videos = searchHelper
            .FilterList(videoLibrary.Videos, _searchText);

        _visibleVideos = searchHelper
            .FilterList(videoLibrary.Videos, _searchText)
            .Take(_loadedItems)
            .ToList();
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

    private void ClearVideos()
    {
        videoLibraryHelper.ClearVideos(ref _videosProgress);
        _visibleVideos.Clear();
    }

    private void RemoveVideo(VideoModel video)
    {
        videoLibraryHelper.RemoveVideo(video);
        _visibleVideos.Remove(video);
    }

    private void UpdateProgress(ref double progressVariable, double value)
    {
        if (Math.Abs(value - progressVariable) < 0.1)
            return;

        progressVariable = value;
        StateHasChanged();
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private string GetDownloadText()
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

    private string GetSearchBarText()
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

    private bool IsVideoNotLoaded(string videoId)
    {
        return videoLibrary.Videos.Any(v => v.VideoId == videoId)is false;
    }

    private bool IsPlaylistUrl()
    {
        return Uri.IsWellFormedUriString(_videoUrl, UriKind.Absolute) && _videoUrl.Contains("list=");
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