using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Library;
using MudBlazor;
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
            await ShowNoVideoErrorMessage();
            return;
        }

        try
        {
            await ShowFFmpegWarningIfNeeded();

            var token = InitializeToken();
            var progress = new Progress<double>(UpdateProgress);

            foreach (var data in _datas)
            {
                if (data is VideoModel video)
                {
                    await DownloadVideo(video, progress, token);
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
            CancelDownload();
        }
    }

    private static bool IsVideoModel()
    {
        return typeof(TData) == typeof(VideoModel);
    }

    private async Task ShowNoVideoErrorMessage()
    {
        await InvokeAsync(() =>
        {
            string errorMessage = GetDictionary()[KeyWords.NoVideoErrorMessage];
            snackbar.Add(errorMessage, Severity.Error);
        });
    }

    private async Task ShowFFmpegWarningIfNeeded()
    {
        if (IsFFmpegInvalid() is false)
        {
            return;
        }

        await InvokeAsync(() =>
        {
            string errorMessage = GetDictionary()[KeyWords.FfmpegErrorMessage];
            snackbar.Add(errorMessage, Severity.Warning);
        });
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
            case Type videoModelType when videoModelType == typeof(VideoModel):
                return await searchHelper.SearchAsync(_datas as List<VideoModel>, searchInput);

            case Type channelModelType when channelModelType == typeof(ChannelModel):
                return await searchHelper.SearchAsync(_datas as List<ChannelModel>, searchInput);

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
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

    private async Task DownloadVideo(VideoModel video, Progress<double> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _downloadingVideoText = video.Title;
        await youtube.DownloadVideoAsync(video.Url, progress, token);

        await AddSuccessDownloadSnackbar();
    }

    private async Task DeleteData(TData data)
    {
        RemoveData(data);

        switch (data)
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                await videoData.DeleteVideoAsync(data as VideoModel);
                break;

            case Type channelModelType when channelModelType == typeof(ChannelModel):
                await channelData.DeleteChannelAsync(data as ChannelModel);
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                await playlistData.DeletePlaylistAsync(data as PlaylistModel);
                break;
            default:
                break;
        }
    }

    private async Task UpdateAllDatas()
    {
        try
        {
            var dataCopy = _datas.ToList();
            var token = tokenHelper.InitializeToken(ref _updateTokenSource);
            foreach (var d in dataCopy)
            {
                token.ThrowIfCancellationRequested();
                await UpdateData(d, token);
            } 

            await AddSuccessUpdatedDataSnackbar();
        }
        catch (OperationCanceledException)
        {
            await AddOperationCancelSnackbar();
        }
        catch
        {
            await AddErrorWhileUpdatingSnackbar();
        }
        finally
        {
            CancelUpdateData();
        }
    }

    private async Task UpdateData(TData data, CancellationToken token)
    {
        switch (data)
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                await UpdateVideo(data, token);
                break;

            case Type channelModelType when channelModelType == typeof(ChannelModel):
                await UpdateChannel(data, token);
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                await UpdatePlaylist(data, token);
                break;
            default:
                break;
        }
    }


    private async Task UpdateVideo(TData video, CancellationToken token)
    {
        var convertedVideo = video as VideoModel;
        var newVideo = await youtube.GetVideoAsync(convertedVideo.Url, token);

        if (newVideo is null)
        {
            RemoveData(video);

            await AddSuccessUpdatedDataSnackbar();

            await videoData.DeleteVideoAsync(convertedVideo);
        }
        else
        {
            await videoData.SetVideoAsync(convertedVideo.Url, convertedVideo.VideoId);
        }
    }

    private async Task UpdateChannel(TData channel, CancellationToken token)
    {
        var convertedChannel = channel as ChannelModel;
        var newChannel = await youtube.GetChannelAsync(convertedChannel.Url, token);
        if (newChannel is null)
        {
            RemoveData(channel);

            await AddNoLongerExistsSnackbar();

            await channelData.DeleteChannelAsync(convertedChannel);
        }
        else
        {
            await channelData.SetChannelAsync(convertedChannel.Url, convertedChannel.ChannelId);
        }
    }

    private async Task UpdatePlaylist(TData playlist, CancellationToken token)
    {
        var convertedPlaylist = playlist as PlaylistModel;
        var newPlaylist = await youtube.GetPlaylistAsync(convertedPlaylist.Url, token);
        if (newPlaylist is null)
        {
            RemoveData(playlist);

            await AddNoLongerExistsSnackbar();

            await playlistData.DeletePlaylistAsync(convertedPlaylist);
        }
        else
        {
            await playlistData.SetPlaylistAsync(convertedPlaylist.Url, convertedPlaylist.PlaylistId);
        }
    }

    private async Task DeleteAllData()
    {
        CloseDialog();
        var datasCopy = _datas.ToList();
        foreach (var d in datasCopy)
        {
            RemoveData(d);
            await DeleteData(d);
        }
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

    private async Task AddSuccessDownloadSnackbar()
    {
        await InvokeAsync(() =>
        {
            string message = GetDictionary()
                [KeyWords.SuccessfullyDownloaded];

            snackbar.Add(message);
        });
    }

    private async Task AddSuccessUpdatedDataSnackbar()
    {
        await InvokeAsync(() =>
        {
            string message = GetDictionary()
                [KeyWords.SuccessfullyUpdatedData];

            snackbar.Add(message);
        });
    }

    private async Task AddErrorWhileUpdatingSnackbar()
    {
        await InvokeAsync(() =>
        {
            string message = GetDictionary()
               [KeyWords.ErrorWhileUpdating];

            snackbar.Add(message, Severity.Error);
        });
    }

    private async Task AddNoLongerExistsSnackbar()
    {
        await InvokeAsync(() =>
        {
            string message = GetDictionary()
               [KeyWords.NoLongerExistsDelete];

            snackbar.Add(message, Severity.Error);
        });
    }

    private void UpdateProgress(double value)
    {
        if (Math.Abs(value - _videosProgress) < 0.1)
        {
            return;
        }

        _videosProgress = value;
        StateHasChanged();
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

        return $"{searchText} {_datas?.Count} {GetDataTypeName()}";
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

        return $"{downloadText} {_datas?.Count} {videoText}s";
    }

    private string GetPageName()
    {
        string name;

        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                name = nameof(ChannelModel);
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                name = nameof(PlaylistModel);
                break;

            case Type videoModelType when videoModelType == typeof(VideoModel):
                name = nameof(VideoModel);
                break;

            default:
                name = "";
                break;
        }

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