using VidFetchLibrary.Models;
using YoutubeExplode.Search;

namespace VidFetch.Pages;

public partial class Search
{
    private const string DefaultDownloadPath = "Download Folder";
    private const string DefaultExtension = ".mp4";
    private CancellationTokenSource videoTokenSource;
    private CancellationTokenSource channelTokenSource;
    private CancellationTokenSource playlistTokenSource;
    private List<string> downloadPaths = new();
    private List<string> videoExtensions = new();
    private string selectedPath = DefaultDownloadPath;
    private string selectedExtension = DefaultExtension;
    private string errorMessage = "";
    private string videoSearchText = "";
    private string playlistSearchText = "";
    private string channelSearchText = "";
    protected override async Task OnInitializedAsync()
    {
        LoadPathsAndExtensions();
        await LoadStates();
    }

    private void LoadPathsAndExtensions()
    {
        downloadPaths = defaultData.GetDownloadPaths();
        videoExtensions = defaultData.GetVideoExtensions();
    }

    private async Task LoadStates()
    {
        selectedPath = await secureStorage.GetAsync(nameof(selectedPath)) ?? DefaultDownloadPath;
        selectedExtension = await secureStorage.GetAsync(nameof(selectedExtension)) ?? DefaultExtension;
    }

    private async Task SaveStates()
    {
        await secureStorage.SetAsync(nameof(selectedPath), selectedPath);
        await secureStorage.SetAsync(nameof(selectedExtension), selectedExtension);
    }

    private async Task SearchVideos()
    {
        if (string.IsNullOrWhiteSpace(videoSearchText)is false)
        {
            var token = tokenHelper.InitializeToken(ref videoTokenSource);
            videoLibrary.VideoResults = await youtube.GetVideosBySearchAsync(videoSearchText, token);
            CancelVideoSearch();
        }
    }

    private async Task SearchPlaylists()
    {
        if (string.IsNullOrWhiteSpace(playlistSearchText)is false)
        {
            var token = tokenHelper.InitializeToken(ref playlistTokenSource);
            videoLibrary.PlaylistResults = await youtube.GetPlaylistsBySearchAsync(playlistSearchText, token);
            CancelPlaylistSearch();
        }
    }

    private async Task SearchChannels()
    {
        if (string.IsNullOrWhiteSpace(channelSearchText)is false)
        {
            var token = tokenHelper.InitializeToken(ref channelTokenSource);
            videoLibrary.ChannelResults = await youtube.GetChannelBySearchAsync(channelSearchText, token);
            CancelChannelSearch();
        }
    }

    private async Task OpenFileLocation()
    {
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        await folderHelper.OpenFolderLocationAsync(selectedPath);
    }

    private async Task OnButtonClick(string path)
    {
        selectedPath = path;
        await SaveStates();
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

    private void FilterVideos()
    {
        videoLibrary.VideoResults = searchHelper.FilterList(videoLibrary.VideoResults, videoSearchText);
    }

    private void FilterChannels()
    {
        videoLibrary.ChannelResults = searchHelper.FilterList(videoLibrary.ChannelResults, channelSearchText);
    }

    private void FilterPlaylists()
    {
        videoLibrary.PlaylistResults = searchHelper.FilterList(videoLibrary.PlaylistResults, playlistSearchText);
    }

    private void RemoveVideo(VideoModel video)
    {
        videoLibrary.VideoResults.Remove(video);
    }

    private void RemoveChannel(ChannelModel channel)
    {
        videoLibrary.ChannelResults.Remove(channel);
    }

    private void RemovePlaylist(PlaylistModel playlist)
    {
        videoLibrary.PlaylistResults.Remove(playlist);
    }

    private void CancelVideoSearch()
    {
        tokenHelper.CancelRequest(ref videoTokenSource);
    }

    private void CancelChannelSearch()
    {
        tokenHelper.CancelRequest(ref channelTokenSource);
    }

    private void CancelPlaylistSearch()
    {
        tokenHelper.CancelRequest(ref playlistTokenSource);
    }

    private string GetButtonClass(string path)
    {
        if (selectedPath == path)
        {
            return "text-success";
        }

        return "text-danger";
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