﻿using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Downloader;

public interface IYoutubeDownloader
{
    Task DownloadPlaylistAsync(string url, string downloadPath, string extension, bool downloadAll, int videoIndex, CancellationToken cancellationToken);
    Task DownloadSelectedVideoAsync(string downloadPath, string extension, PlaylistVideo playlistVideo);
    Task DownloadVideoAsync(string url, string downloadPath, string extension);
    Task<List<PlaylistVideo>> GetPlayListVideosAsync(string url);
    Task<Video> GetVideoAsync(string url);
}