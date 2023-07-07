using Microsoft.AspNetCore.Components;
using MudBlazor;
using VidFetchLibrary.Language;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class IndexPlaylistVideo
{
    [Parameter]
    [EditorRequired]
    public string PlaylistUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const string PageName = nameof(IndexPlaylistVideo);
    private const int ItemsPerPage = 6;

    private SettingsLibrary _settings;
    private CancellationTokenSource _saveAllTokenSource;
    private CancellationTokenSource _playlistTokenSource;
    private CancellationTokenSource _videoTokenSource;
    private PlaylistModel _playlist = new();
    private List<VideoModel> _visibleVideos = new();
    private string _playlistUrl = "";
    private string _searchText = "";
    private string _currentDownloadingVideo = "";
    private string _firstVideoInPlaylistUrl = "";
    private bool _showDialog = false;
    private double _playlistProgress = 0;
    private double _videoProgress = 0;
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);

        _settings = await settingsData.GetSettingsAsync();

        await LoadPlaylistData();
    }

    private async Task LoadPlaylistData()
    {
        if (string.IsNullOrWhiteSpace(PlaylistUrl) is false)
        {
            _playlist = await youtube.GetPlaylistAsync(PlaylistUrl);
        }

        _visibleVideos = videoLibrary.PlaylistVideos.Take(_loadedItems).ToList();
    }

    private void LoadMoreVideos()
    {
        int videosCount = videoLibrary.PlaylistVideos.Count;

        _loadedItems += ItemsPerPage;

        if (_loadedItems > videosCount)
        {
            _loadedItems = videosCount;
        }

        _visibleVideos = videoLibrary.PlaylistVideos
            .Take(_loadedItems)
            .ToList();

        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
    }


    private async Task LoadPlaylist()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_playlistUrl))
            {
                return;
            }

            if (IsUrlPlaylist())
            {
                await LoadPlaylistVideos();

                _showDialog = true;
                _firstVideoInPlaylistUrl = _playlistUrl;

                _visibleVideos = videoLibrary.PlaylistVideos
                    .Take(_loadedItems)
                    .ToList();
            }
            else
            {
                string message = GetDictionary()[KeyWords.EnterPlaylistUrl];
                snackbar.Add(message);
            }

            _playlistUrl = "";
        }
        catch
        {
            string errorMessage = GetDictionary()
                [KeyWords.ErrorWhileLoadingPlaylist];

            snackbar.Add(errorMessage, Severity.Error);
        }
    }

    private async Task LoadPlaylistVideos()
    {
        await OpenLoading.InvokeAsync(true);
        var videos = await youtube.GetPlayListVideosAsync(_playlistUrl);
        foreach (var v in videos)
        {
            if (IsVideoNotLoaded(v.VideoId))
            {
                videoLibrary.PlaylistVideos.Add(v);
            }
        }

        if (_settings.SaveVideos)
        {
            await SavePlaylistVideos();
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task SavePlaylistVideos()
    {
        foreach (var v in videoLibrary.PlaylistVideos)
        {
            await videoData.SetVideoAsync(v.Url, v.VideoId);
        }
    }

    private async Task DownloadAllPlaylists()
    {
        try
        {
            if (IsFFmpegInvalid())
            {
                string errorMessage = GetDictionary()[KeyWords.FfmpegErrorMessage];
                snackbar.Add(errorMessage, Severity.Warning);
            }

            var token = tokenHelper.InitializeToken(ref _playlistTokenSource);

            var progress = new Progress<double>(value =>
            {
                UpdateProgress(ref _playlistProgress, value);
            });

            var videosCopy = _visibleVideos.ToList();

            foreach (var v in videosCopy)
            {
                await DownloadVideo(v, progress, token);
            }

            CancelPlaylistDownload();
        }
        catch (OperationCanceledException)
        {
            string message = GetDictionary()
                [KeyWords.OperationCancelled];

            snackbar.Add(message, Severity.Error);
        }
        catch (Exception ex)
        {
            string errorMessage = GetDictionary(ex.Message)
                [KeyWords.DownloadingErrorMessage];

            snackbar.Add(errorMessage, Severity.Error);

        }
        finally
        {
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task DownloadVideo(VideoModel video, Progress<double> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _currentDownloadingVideo = video.Title;

        await youtube.DownloadVideoAsync(video.Url, progress, token, true, _playlist.Title);

        AddSnackbar(video.Title);

        if (_settings.RemoveAfterDownload)
        {
            videoLibrary.PlaylistVideos.Remove(video);
            _visibleVideos.Remove(video);
        }
    }

    private async Task DownloadFirstVideo(string url)
    {
        try
        {
            if (File.Exists(_settings.FfmpegPath)is false)
            {
                string errorMessage = GetDictionary()[KeyWords.FfmpegErrorMessage];
                snackbar.Add(errorMessage, Severity.Warning);
            }

            var cancellationToken = tokenHelper.InitializeToken(ref _videoTokenSource);

            var progressReport = new Progress<double>(value =>
            {
                UpdateProgress(ref _videoProgress, value);
            });

            await youtube.DownloadVideoAsync(url, progressReport, cancellationToken);
            CancelVideoDownload();
        }
        catch
        {
            string errorMessage = GetDictionary()
                [KeyWords.DownloadingErrorMessage];

            snackbar.Add(errorMessage, Severity.Error);
        }
    }

    private async Task<IEnumerable<string>> SearchPlaylistVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.PlaylistVideos, searchInput);
    }

    private async Task SaveAllVideos()
    {
        try
        {
            var videosCopy = videoLibrary.PlaylistVideos.ToList();
            var token = tokenHelper.InitializeToken(ref _saveAllTokenSource);

            foreach (var v in videosCopy)
            {
                token.ThrowIfCancellationRequested();
                await videoData.SetVideoAsync(v.Url, v.VideoId);
            }

            CancelVideoDownload();

            string successMessage = GetDictionary()
                [KeyWords.SuccessfullySavedAllVideos];

            snackbar.Add(successMessage);
        }
        catch (OperationCanceledException)
        {
            string message = GetDictionary()
                [KeyWords.OperationCancelled];

            snackbar.Add(message, Severity.Error);
        }
        catch
        {
            string errorMessage = GetDictionary()
                [KeyWords.ErrorWhileSaving];

            snackbar.Add(errorMessage, Severity.Error);
        }
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterPlaylistVideo();
    }

    private void FilterPlaylistVideo()
    {
        videoLibrary.PlaylistVideos = searchHelper
            .FilterList(videoLibrary.PlaylistVideos, _searchText);

        _visibleVideos = searchHelper.FilterList(videoLibrary.PlaylistVideos, _searchText)
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

    private void CancelPlaylistDownload()
    {
        tokenHelper.CancelRequest(ref _playlistTokenSource);
        _playlistProgress = 0;
        _currentDownloadingVideo = "";
    }

    private void CancelVideoDownload()
    {
        tokenHelper.CancelRequest(ref _videoTokenSource);
    }

    private void CancelSaveVideo()
    {
        tokenHelper.CancelRequest(ref _saveAllTokenSource);
    }

    private void AddSnackbar(string title)
    {
        string successMessage = GetDictionary(title)
            [KeyWords.SuccessfullyDownloaded];

        snackbar.Add(successMessage);
    }

    private void ClearPlaylistVideos()
    {
        videoLibraryHelper.ClearPlaylistVideos(ref _playlistProgress);
        _visibleVideos.Clear();
    }

    private void RemovePlaylistVideo(VideoModel video)
    {
        videoLibraryHelper.RemovePlaylistVideo(video);
        _visibleVideos.Remove(video);
    }

    private bool IsUrlPlaylist()
    {
        return Uri.IsWellFormedUriString(_playlistUrl, UriKind.Absolute) && _playlistUrl.Contains("list=");
    }

    private bool IsVideoNotLoaded(string videoId)
    {
        return videoLibrary.Videos.Any(v => v.VideoId == videoId)is false;
    }

    private int GetIndex(VideoModel playlistVideo)
    {
        int index = videoLibrary.PlaylistVideos.IndexOf(playlistVideo);
        return index + 1;
    }

    private string GetDownloadText()
    {
        string downloadText = GetDictionary()[KeyWords.Download];
        string videoText = GetDictionary()[KeyWords.Video];

        if (videoLibrary.PlaylistVideos?.Count <= 0)
        {
            return $"{downloadText} {videoText}";
        }

        if (videoLibrary.PlaylistVideos?.Count == 1)
        {
            return $"{downloadText} 1 {videoText}";
        }

        return $"{downloadText} {videoLibrary.PlaylistVideos?.Count} {videoText}";
    }

    private string GetSearchBarText()
    {
        string searchText = GetDictionary()[KeyWords.Search];
        string videoText = GetDictionary()[KeyWords.Video];

        if (videoLibrary?.PlaylistVideos.Count <= 0)
        {
            return $"{searchText} {videoText}";
        }

        if (videoLibrary.PlaylistVideos?.Count == 1)
        {
            return $"{searchText} 1 {videoText}";
        }

        return $"{searchText} {videoLibrary.PlaylistVideos?.Count} {videoText}";
    }

    private Dictionary<KeyWords, string> GetDictionary(string text = "")
    {
        var dictionary = languageExtension.GetDictionary(text);
        return dictionary;
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