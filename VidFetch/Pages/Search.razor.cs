using VidFetchLibrary.Models;

namespace VidFetch.Pages;

public partial class Search
{
    private List<VideoModel> _visibleVideos = new();
    private List<ChannelModel> _visibleChannels = new();
    private List<PlaylistModel> _visiblePlaylists = new();

    private CancellationTokenSource _videoTokenSource;
    private CancellationTokenSource _channelTokenSource;
    private CancellationTokenSource _playlistTokenSource;

    private string _videoUrl = "";
    private string _videoSearchText = "";

    private string _playlistUrl = "";
    private string _playlistSearchText = "";

    private string _channelUrl = "";
    private string _channelSearchText = "";

    private int _loadedVideos = 6;
    private int _loadedChannels = 6;
    private int _loadedPlaylists = 6;

    protected override void OnInitialized()
    {
        _visibleVideos = videoLibrary.VideoResults
            .Take(_loadedVideos)
            .ToList();

        _visibleChannels = videoLibrary.ChannelResults
            .Take(_loadedChannels)
            .ToList();

        _visiblePlaylists = videoLibrary.PlaylistResults
            .Take(_loadedPlaylists)
            .ToList();
    }

    private void LoadMoreVideos()
    {
        int itemsPerPage = 6;
        int videosCount = videoLibrary.VideoResults.Count;

        _loadedVideos += itemsPerPage;

        if (_loadedVideos > videosCount)
        {
            _loadedVideos = videosCount;
        }

        _visibleVideos = videoLibrary.VideoResults.Take(_loadedVideos).ToList();
    }

    private void LoadMoreChannels()
    {
        int itemsPerPage = 6;
        int channelCount = videoLibrary.ChannelResults.Count;

        _loadedChannels += itemsPerPage;

        if (_loadedChannels > channelCount)
        {
            _loadedChannels = channelCount;
        }

        _visibleChannels = videoLibrary.ChannelResults.Take(_loadedChannels).ToList();
    }

    private void LoadMorePlaylists()
    {
        int itemsPerPage = 6;
        int playlistCount = videoLibrary.PlaylistResults.Count;

        _loadedPlaylists += itemsPerPage;

        if (_loadedPlaylists > playlistCount)
        {
            _loadedPlaylists = playlistCount;
        }

        _visiblePlaylists = videoLibrary.PlaylistResults.Take(_loadedPlaylists).ToList();
    }

    private async Task SearchVideos()
    {
        if (string.IsNullOrWhiteSpace(_videoUrl) is false)
        {
            var token = tokenHelper.InitializeToken(ref _videoTokenSource);
            videoLibrary.VideoResults = await youtube.GetVideosBySearchAsync(_videoUrl, token);

            _visibleVideos = videoLibrary.VideoResults.Take(_loadedVideos).ToList();
            CancelVideoSearch();
        }
    }

    private async Task SearchPlaylists()
    {
        if (string.IsNullOrWhiteSpace(_playlistUrl) is false)
        {
            var token = tokenHelper.InitializeToken(ref _playlistTokenSource);
            videoLibrary.PlaylistResults = await youtube.GetPlaylistsBySearchAsync(_playlistUrl, token);

            _visiblePlaylists = videoLibrary.PlaylistResults.Take(_loadedPlaylists).ToList();
            CancelPlaylistSearch();
        }
    }

    private async Task SearchChannels()
    {
        if (string.IsNullOrWhiteSpace(_channelUrl) is false)
        {
            var token = tokenHelper.InitializeToken(ref _channelTokenSource);
            videoLibrary.ChannelResults = await youtube.GetChannelBySearchAsync(_channelUrl, token);

            _visibleChannels = videoLibrary.ChannelResults.Take(_loadedChannels).ToList();
            CancelChannelSearch();
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

    private async Task<IEnumerable<string>> FilterSearchChannels(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.ChannelResults, searchInput);
    }

    private async Task<IEnumerable<string>> FilterSearchPlaylists(string searchInput)
    {
        return await searchHelper.SearchAsync(videoLibrary.PlaylistResults, searchInput);
    }

    private void HandleVideoSearchValueChanged(string value)
    {
        _videoSearchText = value;
        FilterVideos();
    }

    private void HandleChannelSearchValueChanged(string value) 
    {
        _channelSearchText = value;
        FilterChannels();
    }

    private void HandlePlaylistSearchValueChanged(string value)
    {
        _playlistSearchText = value;
        FilterChannels();
    }

    private void FilterVideos()
    {
        videoLibrary.VideoResults = searchHelper
            .FilterList(videoLibrary.VideoResults, _videoSearchText);

        _visibleVideos = searchHelper
            .FilterList(videoLibrary.VideoResults, _videoSearchText)
            .Take(_loadedVideos)
            .ToList();
    }

    private void FilterChannels()
    {
        videoLibrary.ChannelResults = searchHelper
            .FilterList(videoLibrary.ChannelResults, _channelSearchText);

        _visibleChannels = searchHelper
            .FilterList(videoLibrary.ChannelResults, _channelSearchText)
            .Take(_loadedChannels)
            .ToList();
    }

    private void FilterPlaylists()
    {
        videoLibrary.PlaylistResults = searchHelper
            .FilterList(videoLibrary.PlaylistResults, _playlistSearchText);

        _visiblePlaylists = searchHelper
            .FilterList(videoLibrary.PlaylistResults, _playlistSearchText)
            .Take(_loadedPlaylists)
            .ToList();
    }

    private void ClearVideos()
    {
        videoLibrary.VideoResults?.Clear();
    }

    private void ClearChannels()
    {
        videoLibrary.ChannelResults?.Clear();
    }

    private void ClearPlaylists()
    {
        videoLibrary.PlaylistResults?.Clear();
    }

    private void RemoveVideo(VideoModel video)
    {
        videoLibrary.VideoResults.Remove(video);
        _visibleVideos.Remove(video);
    }

    private void RemoveChannel(ChannelModel channel)
    {
        videoLibrary.ChannelResults.Remove(channel);
        _visibleChannels.Remove(channel);
    }

    private void RemovePlaylist(PlaylistModel playlist)
    {
        videoLibrary.PlaylistResults.Remove(playlist);
        _visiblePlaylists.Remove(playlist);
    }

    private void CancelVideoSearch()
    {
        tokenHelper.CancelRequest(ref _videoTokenSource);
    }

    private void CancelChannelSearch()
    {
        tokenHelper.CancelRequest(ref _channelTokenSource);
    }

    private void CancelPlaylistSearch()
    {
        tokenHelper.CancelRequest(ref _playlistTokenSource);
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

    private string GetPlaylistSearchBarText()
    {
        if (videoLibrary.PlaylistResults?.Count <= 0)
        {
            return "Search Playlist";
        }

        if (videoLibrary.PlaylistResults?.Count == 1)
        {
            return "Search 1 Playlist";
        }

        return $"Search {videoLibrary.PlaylistResults?.Count} Playlists";
    }
}