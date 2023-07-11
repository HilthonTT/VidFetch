using VidFetchLibrary.Data;
using VidFetchLibrary.Language;
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
    private int _maxItemsBeingLoaded = 50;

    protected override void OnInitialized()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(GetPageName(), ItemsPerPage);

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

        _visibleData = await searchHelper.GetBySearchAsync(_url, _maxItemsBeingLoaded, token);
        CancelDataSearch();
    }

    private async Task<IEnumerable<string>> FilterSearchData(string searchInput)
    {
        var list = GetList();

        return await filterHelper.FilterSearchData(list, searchInput);
    }

    private void HandleSearchValueChanged(string value)
    {
        _searchText = value;
        FilterData();
    }

    private void FilterData()
    {
        var list = GetList();
        _visibleData = filterHelper.FilterData(list, _searchText, _loadedItems);
    }

    private void ClearData()
    {
        var list = GetList();
        filterHelper.ClearListData(list);

        _visibleData?.Clear();
    }

    private void RemoveData(TData data)
    {
        var list = GetList();

        filterHelper.RemoveData(list, data);
        _visibleData.Remove(data);
    }

    private void CancelDataSearch()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
    }

    private string GetPageName()
    {
        string name = typeof(TData) switch
        {
            Type channel when channel == typeof(ChannelModel) => nameof(ChannelModel),
            Type playlist when playlist == typeof(PlaylistModel) => nameof(PlaylistModel),
            Type video when video == typeof(VideoModel) => nameof(VideoModel),
            _ => "",
        };
        return $"{PageName}-{name}";
    }

    private string GetSingularDataTypeName()
    {
        return typeof(TData) switch
        {
            Type video when video == typeof(VideoModel) => GetDictionary()[KeyWords.Video],
            Type channel when channel == typeof(ChannelModel) => GetDictionary()[KeyWords.Channel],
            Type playlist when playlist == typeof(PlaylistModel) => GetDictionary()[KeyWords.Playlist],
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

    private string GetSearchBarText()
    {
        string dataTypeName = GetSingularDataTypeName();
        string searchText = GetDictionary()[KeyWords.Search];

        if (GetDataResults()?.Count <= 0)
        {
            return $"{searchText} {dataTypeName}";
        }

        if (GetDataResults()?.Count == 1)
        {
            return $"{searchText} 1 {dataTypeName}";
        }

        return $"{searchText} {GetDataResults()?.Count} {dataTypeName}";
    }

    private string GetInputBarHelperText()
    {
        string helperText = GetDictionary()[KeyWords.SearchHelperText];

        string completeHelperText = helperText + GetSingularDataTypeName();
        return completeHelperText;
    }

    private string GetInputBarLabelText()
    {
        string labelText = GetDictionary()[KeyWords.SearchLabelText];

        string completedLabelText = labelText + GetSingularDataTypeName();
        return completedLabelText;
    }

    private string GetClearButtonText()
    {
        string clear = GetDictionary()[KeyWords.Clear];
        string dataTypeName = GetPluralDataTypeName();

        return $"{clear} {dataTypeName}";
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

    private static VideoLibraryList GetList()
    {
        return typeof(TData) switch
        {
            Type channel when channel == typeof(ChannelModel) => VideoLibraryList.ChannelResults,
            Type playlist when playlist == typeof(PlaylistModel) => VideoLibraryList.PlaylistResults,
            Type video when video == typeof(VideoModel) => VideoLibraryList.VideoResults,
            _ => VideoLibraryList.VideoResults,
        };
    }
}