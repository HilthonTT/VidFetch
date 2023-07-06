using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SearchData<TData> where TData : class
{
    private const int ItemsPerPage = 6;
    private const string PageName = nameof(SearchData<TData>);

    private List<TData> _visibleData = new();
    private CancellationTokenSource _tokenSource;
    private string _url = "";
    private string _searchText = "";
    private int _loadedItems = 6;

    protected override void OnInitialized()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(GetPageName(), ItemsPerPage);

        _visibleData = GetDataResults()
            .Take(_loadedItems)
            .ToList();
    }

    private List<TData> GetDataResults()
    {
        if (typeof(TData) == typeof(VideoModel))
        {
            return videoLibrary.VideoResults as List<TData>;
        }
        else if (typeof(TData) == typeof(ChannelModel))
        {
            return videoLibrary.ChannelResults as List<TData>;
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            return videoLibrary.PlaylistResults as List<TData>;
        }

        return new List<TData>();
    }

    private void LoadMoreData()
    {
        int dataCount = GetDataResults().Count;
        _loadedItems += ItemsPerPage;
        
        if (_loadedItems > dataCount)
        {
            _loadedItems = dataCount;
        }

        _visibleData = GetDataResults()
            .Take(_loadedItems)
            .ToList();

        loadedItemsCache.SetLoadedItemsCount(GetPageName(), _loadedItems);
    }

    private async Task SearchTData()
    {
        if (string.IsNullOrWhiteSpace(_url))
        {
            return;
        }

        var token = tokenHelper.InitializeToken(ref _tokenSource);

        if (typeof(TData) == typeof(VideoModel))
        {
            videoLibrary.VideoResults = await youtube.GetVideosBySearchAsync(_url, token);
            _visibleData = videoLibrary.VideoResults as List<TData>;
        }
        else if (typeof(TData) == typeof(ChannelModel))
        {
            videoLibrary.ChannelResults = await youtube.GetChannelsBySearchAsync(_url, token);
            _visibleData = videoLibrary.ChannelResults as List<TData>;
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            videoLibrary.PlaylistResults = await youtube.GetPlaylistsBySearchAsync(_url, token);
            _visibleData = videoLibrary.PlaylistResults as List<TData>;
        }

        CancelDataSearch();
    }

    private async Task<IEnumerable<string>> FilterSearchData(string searchInput)
    {
        if (typeof(TData) == typeof(VideoModel))
        {
            return await searchHelper.SearchAsync(videoLibrary.VideoResults, searchInput);
        }
        else if (typeof(TData) == typeof(ChannelModel))
        {
            return await searchHelper.SearchAsync(videoLibrary.ChannelResults, searchInput);
        }
        else
        {
            return await searchHelper.SearchAsync(videoLibrary.PlaylistResults, searchInput);
        }
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterData();
    }

    private void FilterData()
    {
        if (typeof(TData) == typeof(ChannelModel))
        {
            videoLibrary.ChannelResults = searchHelper
                .FilterList(videoLibrary.ChannelResults, _searchText);

            _visibleData = searchHelper.FilterList(videoLibrary.ChannelResults, _searchText).
                Take(_loadedItems)
                .ToList() as List<TData>;
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            videoLibrary.PlaylistResults = searchHelper
                .FilterList(videoLibrary.PlaylistResults, _searchText);

            _visibleData = searchHelper.FilterList(videoLibrary.PlaylistResults, _searchText)
                .Take(_loadedItems)
                .ToList() as List<TData>;
        }
        else
        {
            videoLibrary.VideoResults = searchHelper
                .FilterList(videoLibrary.VideoResults, _searchText);

            _visibleData = searchHelper.FilterList(videoLibrary.VideoResults, _searchText)
                .Take(_loadedItems)
                .ToList() as List<TData>;
        }
    }

    private void ClearData()
    {
        if (typeof(TData) == typeof(ChannelModel))
        {
            videoLibrary.ChannelResults.Clear();
        }
        else if (typeof(TData) == typeof(PlaylistModel))
        {
            videoLibrary.PlaylistResults.Clear();
        }
        else
        {
            videoLibrary.VideoResults.Clear();
        }

        _visibleData?.Clear();
    }

    private void RemoveData(TData data)
    {
        if (data is ChannelModel channel)
        {
            videoLibrary.ChannelResults.Remove(channel);
        }
        else if (data is PlaylistModel playlist)
        {
            videoLibrary.PlaylistResults.Remove(playlist);
        }
        else if (data is VideoModel video)
        {
            videoLibrary.VideoResults.Remove(video);
        }

        _visibleData.Remove(data);
    }

    private void CancelDataSearch()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
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

    private string GetSearchBarText()
    {
        string dataTypeName = GetDataTypeName();
        if (GetDataResults()?.Count <= 0)
        {
            return $"Search {dataTypeName}";
        }

        if (GetDataResults()?.Count == 1)
        {
            return $"Search 1 {dataTypeName}";
        }

        return $"Search {GetDataResults()?.Count} {dataTypeName}";
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