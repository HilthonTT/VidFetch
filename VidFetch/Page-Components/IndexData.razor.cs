using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Library;
using MudBlazor;
using VidFetchLibrary.Models;
using VidFetchLibrary.Language;

namespace VidFetch.Page_Components;

public partial class IndexData<TData> where TData : class
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    [Parameter]
    public EventCallback SwitchEvent { get; set; }

    [Parameter]
    public EventCallback<string> GetPlaylistUrl { get; set; }

    [Parameter]
    public EventCallback<string> AddVideos { get; set; }

    private const string PageName = nameof(IndexData<TData>);
    private const int ItemsPerPage = 6;

    private SettingsLibrary _settings;
    private CancellationTokenSource _downloadTokenSource;
    private List<TData> _visibleData = new();
    private string _url = "";
    private string _searchText = "";
    private string _downloadingVideoText = "";
    private double _progress = 0;
    private int _loadedItems = 6;

    protected override async Task OnInitializedAsync()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);
        _visibleData = GetDataResults().Take(_loadedItems).ToList();
        _settings = await settingsData.GetSettingsAsync();
    }

    private List<TData> GetDataResults()
    {
        switch (typeof(TData))
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                return videoLibrary.Videos as List<TData>;

            case Type channelModelType when channelModelType == typeof(ChannelModel):
                return videoLibrary.Channels as List<TData>;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                return videoLibrary.Playlists as List<TData>;

            default:
                return new List<TData>();
        }
    }

    private void LoadMoreData()
    {
        int dataCount = GetDataResults().Count;
        _loadedItems += ItemsPerPage;
        if (_loadedItems > dataCount)
        {
            _loadedItems = dataCount;
        }

        _visibleData = GetDataResults().Take(_loadedItems).ToList();
        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
    }

    private async Task DownloadAllVideos()
    {
        try
        {
            if (IsFFmpegInvalid())
            {
                string errorMessage = GetDictionary()[KeyWords.FfmpegErrorMessage];
                snackbar.Add(errorMessage, Severity.Warning);
            }

            var token = tokenHelper.InitializeToken(ref _downloadTokenSource);
            var progressReport = new Progress<double>(UpdateProgress);

            var dataCopy = _visibleData.ToList();

            foreach (var d in dataCopy)
            {
                var video = d as VideoModel;
                await DownloadVideo(video, progressReport, token);

                if (_settings.RemoveAfterDownload)
                {
                    RemoveData(d);
                }
            }
        }
        catch (OperationCanceledException)
        {
            await AddOperationCancelSnackbar();
        }
        catch
        {
            await AddDownloadErrorSnackbar();
        }
        finally
        {
            CancelVideosDownload();
        }
    }

    private async Task DownloadVideo(VideoModel video, Progress<double> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _downloadingVideoText = video.Title;
        await youtube.DownloadVideoAsync(video.Url, progress, token);

        string successMessage = GetDictionary(video.Title)
            [KeyWords.SuccessfullyDownloaded];

        snackbar.Add(successMessage);
    }

    private void CancelVideosDownload()
    {
        tokenHelper.CancelRequest(ref _downloadTokenSource);
        _downloadingVideoText = "";
    }

    private async Task LoadData()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_url))
            {
                return;
            }

            if (IsPlaylistUrl())
            {
                await GetPlaylistUrl.InvokeAsync(_url);
                await AddVideos.InvokeAsync(_url);
                await SwitchEvent.InvokeAsync();
            }
            else
            {
                await LoadSingleData();
            }

            _url = "";
        }
        catch
        {
            string errorMessage = GetDictionary()
                [KeyWords.ErrorWhileLoadingData];

            snackbar.Add(errorMessage, Severity.Error);
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task LoadSingleData()
    {
        await OpenLoading.InvokeAsync(true);

        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                await LoadChannel();
                break;
            case Type videoModelType when videoModelType == typeof(VideoModel):
                await LoadVideo();
                break;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                await LoadPlaylist();
                break;
        }

        if (_settings.SaveVideos)
        {
            await SaveData();
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task LoadChannel()
    {
        var channel = await youtube.GetChannelAsync(_url);

        if (IsDataNotLoaded(channel.ChannelId))
        {
            videoLibrary.Channels.Add(channel);
        }

        _visibleData = videoLibrary.Channels.Take(_loadedItems).ToList() as List<TData>;
    }

    private async Task LoadVideo()
    {
        var video = await youtube.GetVideoAsync(_url);

        if (IsDataNotLoaded(video.VideoId))
        {
            videoLibrary.Videos.Add(video);
        }

        _visibleData = videoLibrary.Videos.Take(_loadedItems).ToList() as List<TData>;
    }

    private async Task LoadPlaylist()
    {
        var playlist = await youtube.GetPlaylistAsync(_url);

        if (IsDataNotLoaded(playlist.PlaylistId))
        {
            videoLibrary.Playlists.Add(playlist);
        }

        _visibleData = videoLibrary.Playlists.Take(_loadedItems).ToList() as List<TData>;
    }

    private async Task SaveData()
    {
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                foreach (var channel in videoLibrary.Channels)
                {
                    await channelData.SetChannelAsync(channel.Url, channel.ChannelId);
                }
                break;

            case Type videoModelType when videoModelType == typeof(VideoModel):
                foreach (var video in videoLibrary.Videos)
                {
                    await videoData.SetVideoAsync(video.Url, video.VideoId);
                }
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                foreach (var playlist in videoLibrary.Playlists)
                {
                    await playlistData.SetPlaylistAsync(playlist.Url, playlist.PlaylistId);
                }
                break;
        }
    }

    private async Task<IEnumerable<string>> FilterSearchData(string searchInput)
    {
        switch (typeof(TData))
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                return await searchHelper.SearchAsync(videoLibrary.Videos, searchInput);

            case Type channelModelType when channelModelType == typeof(ChannelModel):
                return await searchHelper.SearchAsync(videoLibrary.Channels, searchInput);

            case Type videoModelType when videoModelType == typeof(VideoModel):
                return await searchHelper.SearchAsync(videoLibrary.Playlists, searchInput);

            default:
                return default;
        }
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private async Task AddOperationCancelSnackbar()
    {
        await InvokeAsync(() =>
        {
            string message = GetDictionary()
                [KeyWords.OperationCancelled];

            snackbar.Add(message, Severity.Error);
        });
    }

    private async Task AddDownloadErrorSnackbar()
    {
        await InvokeAsync(() =>
        {
            string message = GetDictionary()
                [KeyWords.DownloadingErrorMessage];

            snackbar.Add(message, Severity.Error);
        });
    }

    private void FilterData()
    {
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                videoLibrary.Channels = searchHelper.FilterList(videoLibrary.Channels, _searchText);
                _visibleData = searchHelper.FilterList(videoLibrary.Channels, _searchText)
                    .Take(_loadedItems)
                    .ToList() as List<TData>;
                break;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                videoLibrary.Playlists = searchHelper.FilterList(videoLibrary.Playlists, _searchText);
                _visibleData = searchHelper.FilterList(videoLibrary.Playlists, _searchText)
                    .Take(_loadedItems)
                    .ToList() as List<TData>;
                break;
            case Type videoModelType when videoModelType == typeof(VideoModel):
                videoLibrary.Videos = searchHelper.FilterList(videoLibrary.Videos, _searchText);
                _visibleData = searchHelper.FilterList(videoLibrary.Videos, _searchText)
                    .Take(_loadedItems)
                    .ToList() as List<TData>;
                break;
            default:
                break;
        }
    }

    private void UpdateProgress(double value)
    {
        if (Math.Abs(value - _progress) < 0.1)
        {
            return;
        }

        _progress = value;
        StateHasChanged();
    }

    private void ClearList()
    {
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                videoLibrary.Channels.Clear();
                break;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                videoLibrary.Playlists.Clear();
                break;
            case Type videoModelType when videoModelType == typeof(VideoModel):
                videoLibrary.Videos.Clear();
                break;
            default:
                break;
        }

        _visibleData.Clear();
    }

    private void ClearDatas()
    {
        ClearList();
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                videoLibrary.Channels.Clear();
                break;
            case Type videoModelType when videoModelType == typeof(VideoModel):
                videoLibrary.Videos.Clear();
                break;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                videoLibrary.Playlists.Clear();
                break;
            default:
                break;
        }
    }

    private void RemoveData(TData data)
    {
        RemoveDataFromList(data);
        switch (data)
        {
            case ChannelModel channel:
                videoLibrary.Channels.Remove(channel);
                break;
            case PlaylistModel playlist:
                videoLibrary.Playlists.Remove(playlist);
                break;
            case VideoModel video:
                videoLibrary.Videos.Remove(video);
                break;
        }
    }

    private void RemoveDataFromList(TData data)
    {
        switch (data)
        {
            case ChannelModel channel:
                videoLibrary.Channels.Remove(channel);
                break;
            case PlaylistModel playlist:
                videoLibrary.Playlists.Remove(playlist);
                break;
            case VideoModel video:
                videoLibrary.Videos.Remove(video);
                break;
        }

        _visibleData.Remove(data);
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterData();
    }

    private string GetDataTypeName()
    {
        string typeName = typeof(TData).Name;
        string trimmedName;
        
        if (typeName.EndsWith("Model"))
        {
            trimmedName = typeName[..^"Model".Length];
        }
        else
        {
            trimmedName = typeName;
        }

        return trimmedName switch
        {
            "Video" => GetDictionary()[KeyWords.Video].ToLower(),
            "Channel" => GetDictionary()[KeyWords.Channel].ToLower(),
            "Playlist" => GetDictionary()[KeyWords.Playlist].ToLower(),
            _ => "",
        };
    }

    private string GetDownloadText()
    {
        string downloadText = GetDictionary()[KeyWords.Download];
        string videoText = GetDictionary()[KeyWords.Video];

        if (videoLibrary.Videos?.Count <= 0)
        {
            return $"{downloadText} {videoText}";
        }

        if (videoLibrary.Videos?.Count == 1)
        {
            return $"{downloadText} 1 {videoText}";
        }

        return $"{downloadText} {videoLibrary.Videos?.Count} {videoText}";
    }

    private string GetSearchBarText()
    {
        string searchText = GetDictionary()[KeyWords.Search];

        if (GetDataResults().Count <= 0)
        {
            return $"{searchText} {GetDataTypeName()}";
        }

        if (GetDataResults().Count == 1)
        {
            return $"{searchText} 1 {GetDataTypeName()}";
        }

        return $"{searchText} {GetDataResults().Count} {GetDataTypeName()}";
    }

    private bool IsDataNotLoaded(string dataId)
    {
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                return videoLibrary.Channels.Any(c => c.ChannelId == dataId) is false;
            case Type videoModelType when videoModelType == typeof(VideoModel):
                return videoLibrary.Videos.Any(v => v.VideoId == dataId) is false; ;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                return videoLibrary.Playlists.Any(p => p.PlaylistId == dataId) is false;
            default:
                return false;
        }
    }

    private bool IsPlaylistUrl()
    {
        return Uri.IsWellFormedUriString(_url, UriKind.Absolute) && _url.Contains("list=");
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

    private static bool IsVideoModel()
    {
        if (typeof(TData) == typeof(VideoModel))
        {
            return true;
        }

        return false;
    }

    private Dictionary<KeyWords, string> GetDictionary(string text = "")
    {
        var dictionary = languageExtension.GetDictionary(text);
        return dictionary;
    }

    private List<VideoModel> GetVideos()
    {
        return _visibleData.Take(_loadedItems).ToList() as List<VideoModel>;
    }

    private List<ChannelModel> GetChannels()
    {
        return _visibleData.Take(_loadedItems).ToList() as List<ChannelModel>;
    }

    private List<PlaylistModel> GetPlaylists()
    {
        return _visibleData.Take(_loadedItems).ToList() as List<PlaylistModel>;
    }
}