﻿using VidFetchLibrary.Helpers;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.Client;
public class Youtube : IYoutube
{
    private readonly IDownloadHelper _downloaderHelper;
    private readonly ICachingHelper _cachingHelper;

    public Youtube(IDownloadHelper downloaderHelper,
                   ICachingHelper cachingHelper)
    {
        _downloaderHelper = downloaderHelper;
        _cachingHelper = cachingHelper;
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

    public async Task<List<VideoModel>> GetPlayListVideosAsync(string url)
    {
        var playlistVideos = await _cachingHelper.GetPlayListVideosAsync(url);
        return playlistVideos;
    }

    public async Task<List<VideoModel>> GetChannelVideosAsync(string url)
    {
        var channelVideos = await _cachingHelper.GetChannelVideosAsync(url);
        return channelVideos;
    }

    public async Task<VideoModel> GetVideoAsync(string url)
    {
        var video = await _cachingHelper.GetVideoAsync(url);
        return video;
    }

    public async Task<ChannelModel> GetChannelAsync(string url)
    {
        var channel = await _cachingHelper.GetChannelAsync(url);
        return channel;
    }

    public async Task<PlaylistModel> GetPlaylistAsync(string url)
    {
        var playlist = await _cachingHelper.GetPlaylistAsync(url);
        return playlist;
    }

    public async Task<List<VideoModel>> GetVideosBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        var videos = await _cachingHelper.GetVideosBySearchAsync(searchInput, token);
        return videos;
    }

    public async Task<List<ChannelModel>> GetChannelBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        var channels = await _cachingHelper.GetChannelBySearchAsync(searchInput, token);
        return channels;
    }

    public async Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(
        string searchInput,
        CancellationToken token)
    {
        var playlists = await _cachingHelper.GetPlaylistsBySearchAsync(searchInput, token);
        return playlists;
    }
}
