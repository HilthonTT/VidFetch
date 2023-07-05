using VidFetchLibrary.Models;

namespace VidFetch.Page_Components;

public partial class SearchVideo
{
    private const int ItemsPerPage = 6;
    private const string PageName = nameof(SearchVideo);

    private List<VideoModel> _visibleVideos = new();
    private CancellationTokenSource _tokenSource;
    private string _videoUrl = "";
    private string _searchText = "";
    private int _loadedItems = 6;
    protected override void OnInitialized()
    {
        _loadedItems = loadedItemsCache.GetLoadedItemsCount(PageName, ItemsPerPage);

        _visibleVideos = videoLibrary.VideoResults
            .Take(_loadedItems)
            .ToList();
    }

    private void LoadMoreVideos()
    {
        int itemsPerPage = 6;
        int videosCount = videoLibrary.VideoResults.Count;
        _loadedItems += itemsPerPage;

        if (_loadedItems > videosCount)
        {
            _loadedItems = videosCount;
        }

        _visibleVideos = videoLibrary.VideoResults
            .Take(_loadedItems)
            .ToList();

        loadedItemsCache.SetLoadedItemsCount(PageName, _loadedItems);
    }

    private async Task SearchVideos()
    {
        if (string.IsNullOrWhiteSpace(_videoUrl)is false)
        {
            var token = tokenHelper.InitializeToken(ref _tokenSource);
            videoLibrary.VideoResults = await youtube.GetVideosBySearchAsync(_videoUrl, token);

            _visibleVideos = videoLibrary.VideoResults
                .Take(_loadedItems)
                .ToList();
            
            CancelVideoSearch();
        }
    }

    private async Task OpenFileLocation()
    {
        await folderHelper.OpenFolderLocationAsync();
    }

    private async Task<IEnumerable<string>> FilterSearchVideos(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.VideoResults, searchInput);
    }

    private void HandleVideoSearchValueChanged(string value)
    {
        _searchText = value;
        FilterVideos();
    }

    private void FilterVideos()
    {
        videoLibrary.VideoResults = searchHelper
            .FilterList(videoLibrary.VideoResults, _searchText);

        _visibleVideos = searchHelper.FilterList(videoLibrary.VideoResults, _searchText)
            .Take(_loadedItems)
            .ToList();
    }

    private void ClearVideos()
    {
        videoLibrary.VideoResults?.Clear();
    }

    private void RemoveVideo(VideoModel video)
    {
        videoLibrary.VideoResults.Remove(video);
        _visibleVideos.Remove(video);
    }

    private void CancelVideoSearch()
    {
        tokenHelper.CancelRequest(ref _tokenSource);
    }

    private string GetVideoSearchBarText()
    {
        if (videoLibrary.VideoResults?.Count <= 0)
        {
            return "Search Video";
        }

        if (videoLibrary.VideoResults?.Count == 1)
        {
            return "Search 1 Video";
        }

        return $"Search {videoLibrary.VideoResults?.Count} Videos";
    }
}