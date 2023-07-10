using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;
using VidFetchLibrary.Language;

namespace VidFetch.Page_Components;

public partial class SavedData<TData> where TData : class
{
    [Parameter]
    [EditorRequired]
    public EventCallback<bool> OpenLoading { get; set; }

    private const int ItemsPerPage = 6;
    private const string PageName = nameof(SavedData<TData>);

    private List<TData> _visibleData = new();
    private List<TData> _datas = new();
    private SettingsLibrary _settings;
    private CancellationTokenSource _allVideosTokenSource;
    private CancellationTokenSource _updateTokenSource;
    private string _searchText = "";
    private string _downloadingVideoText = "";
    private double _videosProgress = 0;
    private int _loadedItems = 6;
    private bool _isVisible = false;

    protected override async Task OnInitializedAsync()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(GetPageName(), ItemsPerPage);
        _settings = await settingsData.GetSettingsAsync();
        await LoadDatas();
    }

    private void LoadMoreData()
    {
        int dataCount = _datas.Count;
        _loadedItems += ItemsPerPage;

        if (_loadedItems > dataCount)
        {
            _loadedItems = dataCount;
        }

        _visibleData = _datas
            .Take(_loadedItems)
            .ToList();

        loadedItemsCache.SetLoadedItemsCount(GetPageName(), _loadedItems);
    }

    private async Task LoadDatas()
    {
        await OpenLoading.InvokeAsync(true);
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                _datas = await channelData.GetAllChannelsAsync() as List<TData>;
                break;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                _datas = await playlistData.GetAllPlaylistsAsync() as List<TData>;
                break;
            case Type videoModelType when videoModelType == typeof(VideoModel):
                _datas = await videoData.GetAllVideosAsync() as List<TData>;
                break;
            default:
                break;
        }

        _visibleData = _datas.Take(_loadedItems).ToList();
        await OpenLoading.InvokeAsync(true);
    }

    private async Task DownloadVideos()
    {
        if (IsVideoModel() is false || _datas == null || _datas.Count == 0)
        {
            snackbarHelper.ShowNoVideoErrorMessage();
            return;
        }

        try
        {
            var token = InitializeToken();
            ShowFFmpegWarningIfNeeded();

            var progress = new Progress<double>(async val =>
            {
                await UpdateProgress(val);
            });

            await generalHelper.DownloadAllAsync(_datas, progress, token);
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
            CancelDownload();
        }
    }

    private static bool IsVideoModel()
    {
        return typeof(TData) == typeof(VideoModel);
    }

    private void ShowFFmpegWarningIfNeeded()
    {
        if (IsFFmpegInvalid() is false)
        {
            return;
        }

        snackbarHelper.ShowFfmpegError();
    }

    private CancellationToken InitializeToken()
    {
        return tokenHelper.InitializeToken(ref _allVideosTokenSource);
    }

    private void FilterData()
    {
        _datas = searchHelper.FilterList(_datas, _searchText);
        _visibleData = searchHelper.FilterList(_datas, _searchText)
            .Take(_loadedItems)
            .ToList();
    }

    private async Task<IEnumerable<string>> FilterSearchData(string searchInput)
    {
        switch (typeof(TData))
        {
            case Type video when video == typeof(VideoModel):
                return await searchHelper.SearchAsync(_datas as List<VideoModel>, searchInput);

            case Type channel when channel == typeof(ChannelModel):
                return await searchHelper.SearchAsync(_datas as List<ChannelModel>, searchInput);

            case Type playlist when playlist == typeof(PlaylistModel):
                return await searchHelper.SearchAsync(_datas as List<PlaylistModel>, searchInput);

            default:
                return default;
        }
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterData();
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private async Task UpdateAllDatas()
    {
        try
        {
            var token = tokenHelper.InitializeToken(ref _updateTokenSource);
            var dataCopy = _datas.ToList();
            string dataTypeName = GetPluralDataTypeName().ToLower();

            await generalHelper.UpdateAllDataAsync(dataCopy, token, RemoveData);

            snackbarHelper.ShowSuccessfullyUpdatedDataMessage(dataTypeName);
        }
        catch (OperationCanceledException)
        {
            snackbarHelper.ShowErrorOperationCanceledMessage();
        }
        catch
        {
            snackbarHelper.ShowErrorWhileUpdatingMessage();
        }
        finally
        {
            CancelUpdateData();
        }
    }

    private async Task DeleteAllData()
    {
        CloseDialog();
        var datasCopy = _datas.ToList();
        await generalHelper.DeleteAllAsync(datasCopy, RemoveData);
    }

    private async Task DeleteData(TData data)
    {
        RemoveData(data);
        await generalHelper.DeleteAsync(data);
    }

    private async Task UpdateProgress(double value)
    {
        if (Math.Abs(value - _videosProgress) < 0.1)
        {
            return;
        }

        _videosProgress = value;
        await InvokeAsync(StateHasChanged);
    }

    private void CancelDownload()
    {
        tokenHelper.CancelRequest(ref _allVideosTokenSource);
        _videosProgress = 0;
        _downloadingVideoText = "";
    }

    private void CancelUpdateData()
    {
        tokenHelper.CancelRequest(ref _updateTokenSource);
    }

    private void RemoveData(TData data)
    {
        _visibleData?.Remove(data);
        _datas?.Remove(data);
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
        string searchText = GetDictionary()[KeyWords.Search];

        if (_datas?.Count <= 0)
        {
            return $"{searchText} {GetDataTypeName()}";
        }

        if (_datas?.Count == 1)
        {
            return $"{searchText} 1 {GetDataTypeName()}";
        }

        return $"{searchText} {_datas?.Count} {GetPluralDataTypeName().ToLower()}";
    }

    private string GetDownloadVideoText()
    {
        string videoText = GetDictionary()[KeyWords.Video];
        string downloadText = GetDictionary()[KeyWords.Download];

        if (_datas?.Count <= 0)
        {
            return $"{downloadText} {videoText}";
        }

        if (_datas?.Count == 1)
        {
            return $"{downloadText} 1 {videoText}";
        }

        return $"{downloadText} {_datas?.Count} {GetPluralDataTypeName()}";
    }

    private string GetProgressText()
    {
        string progress = (_videosProgress * 100).ToString("0.##");
        return $"{progress}%";
    }

    private string GetPageName()
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

    private string GetPluralDataTypeName()
    {
        return typeof(TData) switch
        {
            Type video when video == typeof(VideoModel) => GetDictionary()[KeyWords.Videos],
            Type channel when channel == typeof(ChannelModel) => GetDictionary()[KeyWords.Channels],
            Type playlist when playlist == typeof(PlaylistModel) => GetDictionary()[KeyWords.Playlists],
            _ => "",
        };
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