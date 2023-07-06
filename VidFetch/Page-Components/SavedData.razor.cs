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
        if (typeof(TData) == typeof(ChannelModel))
        {
            _datas = await channelData.GetAllChannelsAsync() as List<TData>;
            _visibleData = _datas.Take(_loadedItems).ToList();
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            _datas = await playlistData.GetAllPlaylistsAsync() as List<TData>;
            _visibleData = _datas.Take(_loadedItems).ToList();
        }
        else
        {
            _datas = await videoData.GetAllVideosAsync() as List<TData>;
            _visibleData = _datas.Take(_loadedItems).ToList();
        }

        await OpenLoading.InvokeAsync(true);
    }

    private async Task DownloadVideos()
    {
        bool isVideoModel = typeof(TData) == typeof(VideoModel);
        if (isVideoModel is false || _datas?.Count <= 0)
        {
            snackbar.Add("Error: You have no videos available.", Severity.Error);
            return;
        }

        try
        {
            if (IsFFmpegInvalid())
            {
                string errorMessage = GetDictionary()[KeyWords.FfmpegErrorMessage];
                snackbar.Add(errorMessage, Severity.Warning);
            }

            var token = tokenHelper.InitializeToken(ref _allVideosTokenSource);
            var progress = new Progress<double>(UpdateProgress);

            foreach (var d in _datas)
            {
                var video = d as VideoModel;
                await DownloadVideo(video, progress, token);
            }

            CancelDownload();
        }
        catch (Exception ex)
        {
            snackbar.Add($"There was an issue downloading your videos: {ex.Message}", Severity.Error);
        }
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
        if (typeof(TData) == typeof(VideoModel))
        {
            return await searchHelper.SearchAsync(_datas as List<VideoModel>, searchInput);
        }
        else if (typeof(TData) == typeof(ChannelModel))
        {
            return await searchHelper.SearchAsync(_datas as List<ChannelModel>, searchInput);
        }
        else
        {
            return await searchHelper.SearchAsync(_datas as List<PlaylistModel>, searchInput);
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

        snackbar.Add($"Successfully downloaded {video.Title}");
    }

    private async Task DeleteData(TData data)
    {
        RemoveData(data);
        if (typeof(TData) == typeof(ChannelModel))
        {
            var channel = data as ChannelModel;
            await channelData.DeleteChannelAsync(channel);
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            var playlist = data as PlaylistModel;
            await playlistData.DeletePlaylistAsync(playlist);
        }
        else
        {
            var video = data as VideoModel;
            await videoData.DeleteVideoAsync(video);
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

            CancelUpdateData();
            snackbar.Add("Successfully updated.");
        }
        catch (OperationCanceledException)
        {
            snackbar.Add("Update canceled.", Severity.Error);
        }
        catch
        {
            snackbar.Add("An error occurred while updating.", Severity.Error);
        }
    }

    private async Task UpdateData(TData data, CancellationToken token)
    {
        if (data.GetType() == typeof(VideoModel))
        {
            await UpdateVideo(data, token);
        }
        else if (data.GetType() == typeof(ChannelModel))
        {
            await UpdateChannel(data, token);
        }
        else if (data.GetType() == typeof(PlaylistModel))
        {
            await UpdatePlaylist(data, token);
        }
    }

    private async Task UpdateVideo(TData video, CancellationToken token)
    {
        var convertedVideo = video as VideoModel;
        var newVideo = await youtube.GetVideoAsync(convertedVideo.Url, token);
        if (newVideo is null)
        {
            RemoveData(video);
            snackbar.Add($"{convertedVideo?.Title} no longer exists. It has been deleted", Severity.Error);
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
            snackbar.Add($"{convertedChannel.Title} no longer exists. It has been deleted", Severity.Error);
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
            snackbar.Add($"{convertedPlaylist.Title} no longer exists. It has been deleted", Severity.Error);
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
        if (typeof(TData) == typeof(ChannelModel))
        {
            string name = nameof(ChannelModel);
            return $"{PageName}-{name}";
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            string name = nameof(PlaylistModel);
            return $"{PageName}-{name}";
        }
        else
        {
            string name = nameof(VideoModel);
            return $"{PageName}-{name}";
        }
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

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = languageExtension.GetDictionary();
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