using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Downloader;

public interface IYoutubeDownloader
{
    Task DownloadVideoAsync(string url, string downloadPath, string extension, IProgress<double> progress, CancellationToken token, bool downloadSubtitles = false);
    Task<List<PlaylistVideo>> GetPlayListVideosAsync(string url);
    Task<Video> GetVideoAsync(string url);
}