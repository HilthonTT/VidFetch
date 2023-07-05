using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SearchChannel
{
    private const int ItemsPerPage = 6;
    private const string PageName = nameof(SearchChannel);

    private List<ChannelModel> _visibleChannels = new();
    private CancellationTokenSource _tokenSource;
    private string _channelUrl = "";
    private string _searchText = "";
    private int _loadedItems = 6;

    protected override void OnInitialized()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);

        _visibleChannels = videoLibrary.ChannelResults
            .Take(_loadedItems)
            .ToList();
    }

    private void LoadMoreChannels()
    {
        int channelCount = videoLibrary.ChannelResults.Count;

        _loadedItems += ItemsPerPage;
        if (_loadedItems > channelCount)
        {
            _loadedItems = channelCount;
        }

        _visibleChannels = videoLibrary.ChannelResults
            .Take(_loadedItems)
            .ToList();

        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
    }

    private async Task SearchChannels()
    {
        if (string.IsNullOrWhiteSpace(_channelUrl)is false)
        {
            var token = tokenHelper.InitializeToken(ref _tokenSource);
            videoLibrary.ChannelResults = await youtube.GetChannelBySearchAsync(_channelUrl, token);

            _visibleChannels = videoLibrary.ChannelResults
                .Take(_loadedItems)
                .ToList();

            CancelChannelSearch();
        }
    }

    private async Task<IEnumerable<string>> FilterSearchChannels(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.ChannelResults, searchInput);
    }

    private void HandleChannelSearchValueChanged(string value)
    {
        _searchText = value;
        FilterChannels();
    }

    private void FilterChannels()
    {
        videoLibrary.ChannelResults = searchHelper.FilterList(videoLibrary.ChannelResults, _searchText);
        _visibleChannels = searchHelper.FilterList(videoLibrary.ChannelResults, _searchText).Take(_loadedItems).ToList();
    }

    private void RemoveChannel(ChannelModel channel)
    {
        videoLibrary.ChannelResults.Remove(channel);
        _visibleChannels.Remove(channel);
    }

    private void CancelChannelSearch()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
    }

    private void ClearChannels()
    {
        videoLibrary.ChannelResults?.Clear();
    }

    private string GetChannelSearchBarText()
    {
        if (videoLibrary.ChannelResults?.Count <= 0)
        {
            return "Search Channel";
        }

        if (videoLibrary.ChannelResults?.Count == 1)
        {
            return "Search 1 Channel";
        }

        return $"Search {videoLibrary.ChannelResults?.Count} Videos";
    }
}