using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Downloader;

public interface IYoutubeDownloader
{
    Task DownloadPlaylistAsync(string url, string downloadPath, string extension, bool downloadAll, int videoIndex);
    Task DownloadVideoAsync(string url, string downloadPath, string extension);
    Task<List<PlaylistVideo>> GetPlayListVideosAsync(string url);
    Task<Video> GetVideoAsync(string url);
}