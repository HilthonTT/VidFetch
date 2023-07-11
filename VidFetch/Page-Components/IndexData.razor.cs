using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;
using VidFetchLibrary.Language;
using VidFetchLibrary.Data;
using System.Globalization;

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
        var list = GetList();
        return filterHelper.GetDataResults(list);
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
        if (_visibleData?.Count <= 0)
        {
            return;
        }

        try
        {
            ShowFFmpegWarningIfNeeded();

            var token = tokenHelper.InitializeToken(ref _downloadTokenSource);
            var progress = new Progress<double>(async val =>
            {
                await UpdateProgress(val);
            });

            await generalHelper.DownloadAllAsync(_visibleData, progress, token, RemoveDataIfRemoveAfterDownload);
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
            string url = await clipboard.GetTextAsync();

            if (string.IsNullOrWhiteSpace(url) || IsUrl(url) is false)
            {
                return;
            }

            if (IsPlaylistUrl(url))
            {
                await GetPlaylistUrl.InvokeAsync(url);
                await AddVideos.InvokeAsync(url);
                await SwitchEvent.InvokeAsync();

            }

            await LoadSingleData(url);
        }
        catch
        {
            snackbarHelper.ShowErrorWhileLoadingMessage();
        }
        finally
        {
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task LoadSingleData(string url)
    {
        await OpenLoading.InvokeAsync(true);

        _visibleData = await generalHelper.LoadDataAsync(url, _loadedItems);

        if (_settings.SaveVideos)
        {
            await SaveData();
        }

        await OpenLoading.InvokeAsync(false);
    }

    private async Task SaveData()
    {
        var list = GetList();
        var datas = filterHelper.GetDataResults(list);

        await generalHelper.SaveDataAsync(datas, new());
    }

    private async Task<IEnumerable<string>> FilterSearchData(string searchInput)
    {
        var list = GetList();
        return await filterHelper.FilterSearchData(list, searchInput);
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private void FilterData()
    {
        var list = GetList();
        _visibleData = filterHelper.FilterData(list, _searchText, _loadedItems);
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
        var list = GetList();

        filterHelper.ClearListData(list);
        _visibleData.Clear();
    }

    private void ClearDatas()
    {
        ClearList();
        generalHelper.ClearDatas();
    }

    private void RemoveData(TData data)
    {
        _visibleData.Remove(data);
        generalHelper.RemoveData(data);
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterData();
    }

    private string GetName()
    {
        string name = generalHelper.GetName();
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
        if (GetDataResults().Count <= 0)
        {
            return GetSingularSearchBarText();
        }

        if (GetDataResults().Count == 1)
        {
            return GetSingularSearchBarText();
        }

        string count = GetDataResults().Count.ToString();
        return GetPluralSearchBarText(count);
    }

    private string GetProgressText()
    {
        string progress = (_progress * 100).ToString("0.##");
        return $"{progress}%";
    }

    private string GetSingularSearchBarText()
    {
        return typeof(TData) switch
        {
            Type video when video == typeof(VideoModel) => GetDictionary()[KeyWords.SearchVideo],
            Type channel when channel == typeof(ChannelModel) => GetDictionary()[KeyWords.SearchChannel],
            Type playlist when playlist == typeof(PlaylistModel) => GetDictionary()[KeyWords.SearchPlaylist],
            _ => "",
        };
    }

    private string GetPluralSearchBarText(string text)
    {
        return typeof(TData) switch
        {
            Type video when video == typeof(VideoModel) => GetDictionary(text)[KeyWords.SearchVideoPlural],
            Type channel when channel == typeof(ChannelModel) => GetDictionary(text)[KeyWords.SearchChannelPlural],
            Type playlist when playlist == typeof(PlaylistModel) => GetDictionary(text)[KeyWords.SearchPlaylistPlural],
            _ => "",
        };
    }

    private string GetPluralDataTypeText()
    {
        return typeof(TData) switch
        {
            Type video when video == typeof(VideoModel) => GetDictionary()[KeyWords.Videos],
            Type channel when channel == typeof(ChannelModel) => GetDictionary()[KeyWords.Channels],
            Type playlist when playlist == typeof(PlaylistModel) => GetDictionary()[KeyWords.Playlists],
            _ => "",
        };
    }

    private string GetClearButtonText()
    {
        string clear = GetDictionary()[KeyWords.Clear];
        string dataTypeName = GetPluralDataTypeText();

        return $"{clear} {dataTypeName}";
    }

    private string GetSaveButtonText()
    {
        string save = GetDictionary()[KeyWords.Save];
        string dataTypeName = GetPluralDataTypeText();

        return $"{save} {dataTypeName}";
    }

    private static bool IsPlaylistUrl(string url)
    {
        return IsUrl(url) && url.Contains("list=");
    }

    private static bool IsUrl(string url)
    {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);
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

    private static CultureInfo GetCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        return culture;
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

    private static VideoLibraryList GetList()
    {
        return typeof(TData) switch
        {
            Type channel when channel == typeof(ChannelModel) => VideoLibraryList.Channels,
            Type playlist when playlist == typeof(PlaylistModel) => VideoLibraryList.Playlists,
            Type video when video == typeof(VideoModel) => VideoLibraryList.Videos,
            _ => VideoLibraryList.Videos,
        };
    }
}