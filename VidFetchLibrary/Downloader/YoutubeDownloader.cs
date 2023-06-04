﻿using System.Web;
using VidFetchLibrary.Helpers;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Downloader;
public class YoutubeDownloader : IYoutubeDownloader
{
    private readonly IDownloadHelper _downloaderHelper;

    public YoutubeDownloader(IDownloadHelper downloaderHelper)
    {
        _downloaderHelper = downloaderHelper;
    }

    public async Task DownloadVideoAsync(string url, string downloadPath, string extension)
    { 
        var youtube = new YoutubeClient();

        await _downloaderHelper.DownloadVideoAsync(youtube, url, downloadPath, extension);
    }

    public async Task DownloadSelectedVideoAsync(
        string downloadPath,
        string extension,
        PlaylistVideo playlistVideo)
    {
        var youtube = new YoutubeClient();
        await _downloaderHelper.DownloadSelectedVideoAsync(youtube, downloadPath, extension, playlistVideo);
    }

    public async Task DownloadPlaylistAsync(
        string url,
        string downloadPath,
        string extension,
        bool downloadAll,
        int videoIndex,
        CancellationToken cancellationToken)
    {
        var youtube = new YoutubeClient();

        string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");

        var playlistVideos = await youtube.Playlists.GetVideosAsync(playlistId);
        var videoList = playlistVideos.ToList();

        if (downloadAll)
        {
            await _downloaderHelper.DownloadPlaylistAsync(youtube, videoList, downloadPath, extension, cancellationToken);
        }
        else
        {
            await _downloaderHelper.DownloadVideoFromPlaylistAsync(youtube, videoList, videoIndex, downloadPath, extension);
        }
    }

    public async Task<List<PlaylistVideo>> GetPlayListVideosAsync(string url)
    {
        var youtube = new YoutubeClient();
        string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");

        var playlistVideos = await youtube.Playlists.GetVideosAsync(playlistId);
        return playlistVideos.ToList();
    }

    public async Task<Video> GetVideoAsync(string url)
    {
        var youtube = new YoutubeClient();
        return await youtube.Videos.GetAsync(url);
    }

    private static string GetPlaylistId(string url) 
    {
        string queryString = new Uri(url).Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);
        return queryParams.Get("list");
    }
}
