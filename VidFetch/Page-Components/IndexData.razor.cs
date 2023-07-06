using Microsoft.AspNetCore.Components;
using VidFetchLibrary.Library;
using MudBlazor;
using VidFetchLibrary.Models;

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

    private const string FfmpegErrorMessage = "Your ffmpeg path is invalid: Your video resolution might be lower.";
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
        if (typeof(TData) == typeof(VideoModel))
        {
            return videoLibrary.Videos as List<TData>;
        }
        else if (typeof(TData) == typeof(ChannelModel))
        {
            return videoLibrary.Channels as List<TData>;
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            return videoLibrary.Playlists as List<TData>;
        }

        return new List<TData>();
    }

    private void LoadDatas()
    {
        if (typeof(TData) == typeof(ChannelModel))
        {
            _visibleData = GetDataResults().Take(_loadedItems).ToList();
        }
        else if (typeof(TData) == typeof(VideoModel))
        {
            _visibleData = GetDataResults().Take(_loadedItems).ToList();
        }
        else
        {
            _visibleData = GetDataResults().Take(_loadedItems).ToList();
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
                snackbar.Add(FfmpegErrorMessage, Severity.Warning);
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

        _downloadingVideoText = video.Title;
        await youtube.DownloadVideoAsync(video.Url, progress, token);

        snackbar.Add($"Successfully downloaded {video.Title}");
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
        catch (Exception ex)
        {
            snackbar.Add($"Error: {ex.Message}", Severity.Error);
            await OpenLoading.InvokeAsync(false);
        }
    }

    private async Task LoadSingleData()
    {
        await OpenLoading.InvokeAsync(true);

        if (typeof(TData) == typeof(ChannelModel))
        {
            await LoadChannel();
        }
        else if (typeof(TData) == typeof(VideoModel))
        {
            await LoadVideo();
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            await LoadPlaylist();
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
        if (typeof(TData) == typeof(ChannelModel))
        {
            foreach (var channel in videoLibrary.Channels)
            {
                await channelData.SetChannelAsync(channel.Url, channel.ChannelId);
            }
        }
        else if (typeof(TData) == typeof(VideoModel))
        {
            foreach (var video in videoLibrary.Videos)
            {
                await videoData.SetVideoAsync(video.Url, video.VideoId);
            }
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            foreach (var playlist in videoLibrary.Playlists)
            {
                await playlistData.SetPlaylistAsync(playlist.Url, playlist.PlaylistId);
            }
        }
    }

    private async Task<IEnumerable<string>> FilterSearchData(string searchInput)
    {
        if (typeof(TData) == typeof(VideoModel))
        {
            return await searchHelper.SearchAsync(videoLibrary.Videos, searchInput);
        }
        else if (typeof(TData) == typeof(ChannelModel))
        {
            return await searchHelper.SearchAsync(videoLibrary.Channels, searchInput);
        }
        else
        {
            return await searchHelper.SearchAsync(videoLibrary.Playlists, searchInput);
        }
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private void FilterData()
    {
        if (typeof(TData) == typeof(ChannelModel))
        {
            videoLibrary.Channels = searchHelper
                .FilterList(videoLibrary.Channels, _searchText);

            _visibleData = searchHelper
                .FilterList(videoLibrary.Channels, _searchText)
                .Take(_loadedItems)
                .ToList() as List<TData>;
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            videoLibrary.Playlists = searchHelper
                .FilterList(videoLibrary.Playlists, _searchText);

            _visibleData = searchHelper
                .FilterList(videoLibrary.Playlists, _searchText)
                .Take(_loadedItems)
                .ToList() as List<TData>;
        }
        else
        {
            videoLibrary.Videos = searchHelper
                .FilterList(videoLibrary.Videos, _searchText);

            _visibleData = searchHelper
                .FilterList(videoLibrary.Videos, _searchText)
                .Take(_loadedItems)
                .ToList() as List<TData>;
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
        if (typeof(TData) == typeof(ChannelModel))
        {
            videoLibrary.Channels.Clear();
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            videoLibrary.Playlists.Clear();
        }
        else
        {
            videoLibrary.Videos.Clear();
        }

        _visibleData.Clear();
    }

    private void ClearDatas()
    {
        ClearList();
        if (typeof(TData) == typeof(ChannelModel))
        {
            videoLibrary.Channels.Clear();
        }
        else if (typeof(TData) == typeof(VideoModel))
        {
            videoLibrary.Videos.Clear();
        }
        else
        {
            videoLibrary.Playlists.Clear();
        }
    }

    private void RemoveData(TData data)
    {
        RemoveDataFromList(data);
        if (typeof(TData) == typeof(ChannelModel))
        {
            var channel = data as ChannelModel;
            videoLibrary.Channels.Remove(channel);
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            var playlist = data as PlaylistModel;
            videoLibrary.Playlists.Remove(playlist);
        }
        else
        {
            var video = data as VideoModel;
            videoLibrary.Videos.Remove(video);
        }
    }

    private void RemoveDataFromList(TData data)
    {
        if (data is ChannelModel channel)
        {
            videoLibrary.Channels.Remove(channel);
        }
        else if (data is PlaylistModel playlist)
        {
            videoLibrary.Playlists.Remove(playlist);
        }
        else if (data is VideoModel video)
        {
            videoLibrary.Videos.Remove(video);
        }

        _visibleData.Remove(data);
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterData();
    }

    private static string GetDataTypeName()
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

        return trimmedName;
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
        if (GetDataResults().Count <= 0)
        {
            return $"Search {GetDataTypeName()}";
        }

        if (GetDataResults().Count == 1)
        {
            return $"Search 1 {GetDataTypeName()}";
        }

        return $"Search {GetDataResults().Count} {GetDataTypeName()}";
    }

    private bool IsDataNotLoaded(string dataId)
    {
        if (typeof(TData) == typeof(ChannelModel))
        {
            return videoLibrary.Channels.Any(c => c.ChannelId == dataId) is false;
        }
        else if (typeof(TData) == typeof(VideoModel))
        {
            return videoLibrary.Videos.Any(v => v.VideoId == dataId) is false;
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            return videoLibrary.Playlists.Any(p => p.PlaylistId == dataId) is false;
        }

        return false;
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