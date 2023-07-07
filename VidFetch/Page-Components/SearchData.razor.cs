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

    protected override void OnInitialized()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(GetPageName(), ItemsPerPage);

        _visibleData = GetDataResults()
            .Take(_loadedItems)
            .ToList();
    }

    private List<TData> GetDataResults()
    {
        switch (typeof(TData))
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                return videoLibrary.VideoResults as List<TData>;
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                return videoLibrary.ChannelResults as List<TData>;
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                return videoLibrary.PlaylistResults as List<TData>;
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

        switch (typeof(TData))
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                videoLibrary.VideoResults = await youtube.GetVideosBySearchAsync(_url, token);
                _visibleData = videoLibrary.VideoResults as List<TData>;
                break;

            case Type channelModelType when channelModelType == typeof(ChannelModel):
                videoLibrary.ChannelResults = await youtube.GetChannelsBySearchAsync(_url, token);
                _visibleData = videoLibrary.ChannelResults as List<TData>;
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                videoLibrary.PlaylistResults = await youtube.GetPlaylistsBySearchAsync(_url, token);
                _visibleData = videoLibrary.PlaylistResults as List<TData>;
                break;
        }

        CancelDataSearch();
    }

    private async Task<IEnumerable<string>> FilterSearchData(string searchInput)
    {
        switch (typeof(TData))
        {
            case Type videoModelType when videoModelType == typeof(VideoModel):
                return await searchHelper.SearchAsync(videoLibrary.VideoResults, searchInput);
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                return await searchHelper.SearchAsync(videoLibrary.ChannelResults, searchInput);
            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                return await searchHelper.SearchAsync(videoLibrary.PlaylistResults, searchInput);
            default:
                return default;
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
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                videoLibrary.ChannelResults = searchHelper.FilterList(videoLibrary.ChannelResults, _searchText);
                _visibleData = searchHelper.FilterList(videoLibrary.ChannelResults, _searchText)
                    .Take(_loadedItems)
                    .ToList() as List<TData>;
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                videoLibrary.PlaylistResults = searchHelper.FilterList(videoLibrary.PlaylistResults, _searchText);
                _visibleData = searchHelper.FilterList(videoLibrary.PlaylistResults, _searchText)
                    .Take(_loadedItems)
                    .ToList() as List<TData>;
                break;

            case Type videoModelType when videoModelType == typeof(VideoModel):
                videoLibrary.VideoResults = searchHelper.FilterList(videoLibrary.VideoResults, _searchText);
                _visibleData = searchHelper.FilterList(videoLibrary.VideoResults, _searchText)
                    .Take(_loadedItems)
                    .ToList() as List<TData>;
                break;
        }
    }

    private void ClearData()
    {
        switch (typeof(TData))
        {
            case Type channelModelType when channelModelType == typeof(ChannelModel):
                videoLibrary.ChannelResults.Clear();
                break;

            case Type playlistModelType when playlistModelType == typeof(PlaylistModel):
                videoLibrary.PlaylistResults.Clear();
                break;

            case Type videoModelType when videoModelType == typeof(VideoModel):
                videoLibrary.VideoResults.Clear();
                break;

            default:
                break;
        }

        _visibleData?.Clear();
    }

    private void RemoveData(TData data)
    {
        switch (data)
        {
            case ChannelModel channel:
                videoLibrary.ChannelResults.Remove(channel);
                break;
            case PlaylistModel playlist:
                videoLibrary.PlaylistResults.Remove(playlist);
                break;
            case VideoModel video:
                videoLibrary.VideoResults.Remove(video);
                break;
        }

        _visibleData.Remove(data);
    }

    private void CancelDataSearch()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
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

    private string GetSearchBarText()
    {
        string dataTypeName = GetDataTypeName();
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

        string completeHelperText = helperText + GetDataTypeName();
        return completeHelperText;
    }

    private string GetInputBarLabelText()
    {
        string labelText = GetDictionary()[KeyWords.SearchLabelText];

        string completedLabelText = labelText + GetDataTypeName();
        return completedLabelText;
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