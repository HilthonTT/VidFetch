using VidFetchLibrary.Cache;
using VidFetchLibrary.Helpers;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.Client;
public class Youtube : IYoutube
{
    private readonly IDownloadHelper _downloaderHelper;
    private readonly IPlaylistCache _playlistCache;
    private readonly IVideoCache _videoCache;
    private readonly IChannelCache _channelCache;

    public Youtube(IDownloadHelper downloaderHelper,
                   IPlaylistCache playlistCache,
                   IVideoCache videoCache,
                   IChannelCache channelCache)
    {
        _downloaderHelper = downloaderHelper;
        _playlistCache = playlistCache;
        _videoCache = videoCache;
        _channelCache = channelCache;
    }


    public async Task DownloadVideoAsync(
        string url,
        IProgress<double> progress,
        CancellationToken token,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        await _downloaderHelper.DownloadVideoAsync(
            url,
            progress,
            token,
            isPlaylist,
            playlistTitle);
    }

    public async Task<List<VideoModel>> GetPlayListVideosAsync(
        string url,
        CancellationToken token = default)
    {
        var playlistVideos = await _videoCache.GetPlayListVideosAsync(url, token);
        return playlistVideos;
    }

    public async Task<List<VideoModel>> GetChannelVideosAsync(
        string url,
        CancellationToken token = default)
    {
        var channelVideos = await _videoCache.GetChannelVideosAsync(url, token);
        return channelVideos;
    }

    public async Task<VideoModel> GetVideoAsync(
        string url,
        CancellationToken token = default)
    {
        var video = await _videoCache.GetVideoAsync(url, token);
        return video;
    }

    public async Task<ChannelModel> GetChannelAsync(
        string url,
        CancellationToken token = default)
    {
        var channel = await _channelCache.GetChannelAsync(url, token);
        return channel;
    }

    public async Task<PlaylistModel> GetPlaylistAsync(
        string url,
        CancellationToken token = default)
    {
        var playlist = await _playlistCache.GetPlaylistAsync(url, token);
        return playlist;
    }

    public async Task<List<VideoModel>> GetVideosBySearchAsync(
        string searchInput,
        int count,
        CancellationToken token)
    {
        var videos = await _videoCache.GetVideosBySearchAsync(searchInput, count, token);
        return videos;
    }

    public async Task<List<ChannelModel>> GetChannelsBySearchAsync(
        string searchInput,
        int count,
        CancellationToken token)
    {
        var channels = await _channelCache.GetChannelBySearchAsync(searchInput, count, token);
        return channels;
    }

    public async Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(
        string searchInput,
        int count,
        CancellationToken token)
    {
        var playlists = await _playlistCache.GetPlaylistsBySearchAsync(searchInput, count, token);
        return playlists;
    }
}
