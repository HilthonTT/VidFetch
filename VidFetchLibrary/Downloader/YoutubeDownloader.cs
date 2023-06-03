using System.Web;
using VidFetchLibrary.Helpers;
using YoutubeExplode;
using YoutubeExplode.Common;

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

    public async Task DownloadPlaylistAsync(
        string url,
        string downloadPath,
        string extension,
        bool downloadAll,
        int videoIndex)
    {
        var youtube = new YoutubeClient();

        string playlistId = GetPlaylistId(url) ?? throw new Exception("Invalid playlist URL.");
        var playlist = await youtube.Playlists.GetAsync(playlistId) ?? throw new Exception("Playlist not found.");

        var playlistVideos = await youtube.Playlists.GetVideosAsync(playlistId);
        var videoList = playlistVideos.ToList();

        if (downloadAll)
        {
            await _downloaderHelper.DownloadPlaylistAsync(youtube, videoList, downloadPath, extension);
        }
        else
        {
            await _downloaderHelper.DownloadVideoFromPlaylistAsync(youtube, videoList, videoIndex, downloadPath, extension);
        }
    }

    private static string GetPlaylistId(string url) 
    {
        string queryString = new Uri(url).Query;
        var queryParams = HttpUtility.ParseQueryString(queryString);
        return queryParams.Get("list");
    }
}
