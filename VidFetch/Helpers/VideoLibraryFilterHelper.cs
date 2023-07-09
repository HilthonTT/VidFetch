using VidFetchLibrary.Data;
using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

namespace VidFetch.Helpers;
public class VideoLibraryFilterHelper<TData> : IVideoLibraryFilterHelper<TData> where TData : class
{
    private readonly IVideoLibrary _videoLibrary;
    private readonly ISearchHelper<TData> _searchHelper;

    public VideoLibraryFilterHelper(IVideoLibrary videoLibrary,
                                    ISearchHelper<TData> searchHelper)
    {
        _videoLibrary = videoLibrary;
        _searchHelper = searchHelper;
    }

    public async Task<IEnumerable<string>> FilterSearchData(VideoLibraryList list, string searchInput)
    {
        var dataList = GetVideoLibrary(list);

        return await _searchHelper.SearchAsync(dataList, searchInput);
    }

    public List<TData> GetDataResults(VideoLibraryList list)
    {
        var dataList = GetVideoLibrary(list);

        return dataList;
    }

    public List<TData> FilterData(VideoLibraryList list, string searchText, int loadedItems)
    {
        FilterList(list, searchText);

        var dataList = GetVideoLibrary(list);
        dataList = _searchHelper.FilterList(dataList, searchText);

        return dataList.Take(loadedItems).ToList();
    }

    public void ClearListData(VideoLibraryList list)
    {
        switch (list)
        {
            case VideoLibraryList.Videos:
                _videoLibrary.Videos.Clear();
                break;
            case VideoLibraryList.Channels:
                _videoLibrary.Channels.Clear();
                break;
            case VideoLibraryList.VideoResults:
                _videoLibrary.VideoResults.Clear();
                break;
            case VideoLibraryList.ChannelResults:
                _videoLibrary.ChannelResults.Clear();
                break;
            case VideoLibraryList.PlaylistResults:
                _videoLibrary.PlaylistResults.Clear();
                break;
            case VideoLibraryList.PlaylistVideos:
                _videoLibrary.PlaylistVideos.Clear();
                break;
        }
    }

    public void RemoveData(VideoLibraryList list, TData data)
    {
        switch (list)
        {
            case VideoLibraryList.Videos:
                _videoLibrary.Videos.Remove(data as VideoModel);
                break;
            case VideoLibraryList.Channels:
                _videoLibrary.Channels.Remove(data as ChannelModel);
                break;
            case VideoLibraryList.VideoResults:
                _videoLibrary.VideoResults.Remove(data as VideoModel);
                break;
            case VideoLibraryList.ChannelResults:
                _videoLibrary.ChannelResults.Remove(data as ChannelModel);
                break;
            case VideoLibraryList.PlaylistResults:
                _videoLibrary.PlaylistResults.Remove(data as PlaylistModel);
                break;
            case VideoLibraryList.PlaylistVideos:
                _videoLibrary.PlaylistVideos.Remove(data as VideoModel);
                break;
        }
    }

    private void FilterList(VideoLibraryList list, string searchText)
    {
        switch (list)
        {
            case VideoLibraryList.Videos:
                _videoLibrary.Videos = _searchHelper
                    .FilterList(_videoLibrary.Videos, searchText);
                break;
            case VideoLibraryList.Channels:
                _videoLibrary.Channels = _searchHelper
                    .FilterList(_videoLibrary.Channels, searchText);
                break;
            case VideoLibraryList.VideoResults:
                _videoLibrary.VideoResults = _searchHelper
                    .FilterList(_videoLibrary.VideoResults, searchText);
                break;
            case VideoLibraryList.ChannelResults:
                _videoLibrary.ChannelResults = _searchHelper
                    .FilterList(_videoLibrary.ChannelResults, searchText);
                break;
            case VideoLibraryList.PlaylistResults:
                _videoLibrary.PlaylistResults = _searchHelper
                    .FilterList(_videoLibrary.PlaylistResults, searchText);
                break;
            case VideoLibraryList.PlaylistVideos:
                _videoLibrary.PlaylistVideos = _searchHelper
                    .FilterList(_videoLibrary.PlaylistVideos, searchText);
                break;
        }
    }

    private List<TData> GetVideoLibrary(VideoLibraryList list)
    {
        return list switch
        {
            VideoLibraryList.Videos => _videoLibrary.Videos as List<TData>,
            VideoLibraryList.Channels => _videoLibrary.Channels as List<TData>,
            VideoLibraryList.Playlists => _videoLibrary.Playlists as List<TData>,
            VideoLibraryList.VideoResults => _videoLibrary.VideoResults as List<TData>,
            VideoLibraryList.ChannelResults => _videoLibrary.ChannelResults as List<TData>,
            VideoLibraryList.PlaylistResults => _videoLibrary.PlaylistResults as List<TData>,
            VideoLibraryList.PlaylistVideos => _videoLibrary.PlaylistVideos as List<TData>,
            _ => new(),
        };
    }
}
