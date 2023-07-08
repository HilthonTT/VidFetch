using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Library;
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
        InitializeVisibleData();
        _settings = await settingsData.GetSettingsAsync();
    }

    private void InitializeVisibleData()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(GetName(), ItemsPerPage);

        _visibleData = GetDataResults()
            .Take(_loadedItems)
            .ToList();
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
        loadedItemsCache.SetLoadedItemsCount(GetName(), _loadedItems);
    }

    private async Task DownloadAllVideos()
    {
        try
        {
            ShowFFmpegWarningIfNeeded();

            var token = InitializeTokenDownload();
            var progress = new Progress<double>(async val =>
            {
                await UpdateProgress(val);
            });

            await dataHelper.DownloadAllVideosAsync(_visibleData, progress, token, RemoveDataIfRemoveAfterDownload);
        }
        catch (OperationCanceledException)
        {
            snackbarHelper.ShowErrorOperationCanceledMessage();
        }
        catch
        {
            snackbarHelper.ShowErrorDownloadMessage();
        }
        finally
        {
            CancelVideosDownload();
        }
    }

    private void RemoveDataIfRemoveAfterDownload(TData data)
    {
        if (_settings.RemoveAfterDownload)
        {
            RemoveData(data);
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

    private CancellationToken InitializeTokenDownload()
    {
        return tokenHelper.InitializeToken(ref _downloadTokenSource);
    }

    private void CancelVideosDownload()
    {
        tokenHelper.CancelRequest(ref _downloadTokenSource);
        _downloadingVideoText = "";
        _progress = 0;
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

            ResetUrl();
        }
        catch
        {
            await InvokeAsync(snackbarHelper.ShowErrorWhileLoadingMessage);
        }
        finally
        {
            ResetUrl();
        }
    }

    private void ResetUrl()
    {
        _url = "";
    }

    private async Task LoadSingleData()
    {
        await OpenLoading.InvokeAsync(true);

        _visibleData = await dataHelper.LoadDataAsync(_url, _loadedItems);

        if (_settings.SaveVideos)
        {
            await SaveData();
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task SaveData()
    {
        var datas = new List<TData>();

        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                datas = videoLibrary.Channels.ToList() as List<TData>;
                break;

            case Type videoModelType when videoModelType == typeof(VideoModel):
                datas = videoLibrary.Videos.ToList() as List<TData>;
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                datas = videoLibrary.Playlists.ToList() as List<TData>;
                break;
            default:
                break;
        }

        await dataHelper.SaveAllAsync(datas, new());
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

    private async Task UpdateProgress(double value)
    {
        if (Math.Abs(value - _progress) < 0.1)
        {
            return;
        }

        _progress = value;

        await InvokeAsync(StateHasChanged);
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
        dataHelper.ClearDatas();
    }

    private void RemoveData(TData data)
    {
        _visibleData.Remove(data);
        dataHelper.RemoveData(data);
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterData();
    }

    private string GetName()
    {
        string name = dataHelper.GetName();
        return $"{PageName}-{name}";
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