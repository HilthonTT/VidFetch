using Microsoft.AspNetCore.Components;
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
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(GetName(), ItemsPerPage);

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

        loadedItemsCache.SetLoadedItemsCount(GetName(), _loadedItems);
    }


    private async Task LoadPlaylist()
    {
        if (string.IsNullOrWhiteSpace(_playlistUrl))
        {
            return;
        }

        try
        {
            if (IsUrlPlaylist())
            {
                await LoadPlaylistVideos();
                PreparePlaylistDisplay();
            }
            else
            {
                await InvokeAsync(snackbarHelper.ShowEnterPlaylistUrl);
            }

            ResetPlaylistUrl();
        }
        catch
        {
            await InvokeAsync(snackbarHelper.ShowErrorLoadingPlaylist);
        }
    }

    private void PreparePlaylistDisplay()
    {
        _showDialog = true;
        _firstVideoInPlaylistUrl = _playlistUrl;

        _visibleVideos = videoLibrary.PlaylistVideos
            .Take(_loadedItems)
            .ToList();
    }

    private void ResetPlaylistUrl()
    {
        _playlistUrl = "";
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
            await genericHelper.SaveDataAsync(videoLibrary.PlaylistVideos, new CancellationToken());
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task DownloadAllPlaylists()
    {
        if (videoLibrary?.PlaylistVideos?.Count <= 0)
        {
            return;
        }

        try
        {
            await InvokeAsync(ShowFFmpegWarningIfNeeded);

            var token = InitializeTokenForPlaylists();
            var progress = new Progress<double>(async val =>
            {
                await UpdatePlaylistProgress(val);
            });

            await genericHelper.DownloadAllAsync(_visibleVideos, progress, token);
        }
        catch (OperationCanceledException)
        {
            await InvokeAsync(snackbarHelper.ShowErrorOperationCanceledMessage);
        }
        catch
        {
            await InvokeAsync(snackbarHelper.ShowErrorDownloadMessage);
        }
        finally
        {
            CancelPlaylistDownload();
            await OpenLoading.InvokeAsync(false);
        }
    }

    private void ShowFFmpegWarningIfNeeded()
    {
        if (IsFFmpegInvalid() is false)
        {
            return;
        }

        snackbarHelper.ShowFfmpegError();
    }

    private CancellationToken InitializeTokenForPlaylists()
    {
        return tokenHelper.InitializeToken(ref _playlistTokenSource);
    }


    private async Task DownloadFirstVideo(string url)
    {
        try
        {
            await InvokeAsync(ShowFFmpegWarningIfNeeded);

            var cancellationToken = tokenHelper.InitializeToken(ref _videoTokenSource);

            var progressReport = new Progress<double>(async value =>
            {
                await UpdateVideoProgress(value);
            });

            await youtube.DownloadVideoAsync(url, progressReport, cancellationToken);
            await InvokeAsync(snackbarHelper.ShowSuccessfullySavedVideosMessage);
        }
        catch
        {
            await InvokeAsync(snackbarHelper.ShowErrorDownloadMessage);
        }
        finally
        {
            CancelVideoDownload();
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
            var token = InitializeSaveAllToken();

            await genericHelper.SaveDataAsync(videosCopy, token);
        }
        catch (OperationCanceledException)
        {
            await InvokeAsync(snackbarHelper.ShowErrorOperationCanceledMessage);
        }
        catch
        {
            await InvokeAsync(snackbarHelper.ShowErrorWhileSavingMessage);
        }
        finally
        {
            CancelVideoDownload();
        }
    }

    private CancellationToken InitializeSaveAllToken()
    {
        return tokenHelper.InitializeToken(ref _saveAllTokenSource);
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

    private async Task UpdatePlaylistProgress(double value)
    {

        if (Math.Abs(value - _playlistProgress) < 0.1)
        {
            return;
        }

        _playlistProgress = value;
        await InvokeAsync(StateHasChanged);
    }

    private async Task  UpdateVideoProgress(double value)
    {
        if (Math.Abs(value - _videoProgress) < 0.1)
        {
            return;
        }

        _videoProgress = value;
        await InvokeAsync(StateHasChanged);
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

    private void ClearPlaylistVideos()
    {
        videoLibrary.PlaylistVideos.Clear();
        _visibleVideos.Clear();
    }

    private void RemovePlaylistVideo(VideoModel video)
    {
        videoLibrary.PlaylistVideos.Remove(video);
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

    private string GetName()
    {
        string name = genericHelper.GetName();
        return $"{PageName}-{name}";
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