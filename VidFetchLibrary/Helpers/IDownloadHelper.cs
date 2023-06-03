using YoutubeExplode;
using YoutubeExplode.Playlists;

namespace VidFetchLibrary.Helpers;
public interface IDownloadHelper
{
    Task DownloadPlaylistAsync(YoutubeClient client, List<PlaylistVideo> playlistVideos, string path, string extension);
    Task DownloadVideoAsync(YoutubeClient client, string videoUrl, string path, string extension);
    Task DownloadVideoFromPlaylistAsync(YoutubeClient client, List<PlaylistVideo> playlistVideos, int videoIndex, string path, string extension);
}