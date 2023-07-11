using VidFetchLibrary.Models;

namespace VidFetchLibrary.Client;
public interface IYoutube
{
    Task DownloadVideoAsync(string url, IProgress<double> progress, CancellationToken token, bool isPlaylist = false, string playlistTitle = "");
    Task<ChannelModel> GetChannelAsync(string url, CancellationToken token = default);
    Task<List<ChannelModel>> GetChannelsBySearchAsync(string searchInput, int count, CancellationToken token);
    Task<List<VideoModel>> GetChannelVideosAsync(string url, CancellationToken token = default);
    Task<PlaylistModel> GetPlaylistAsync(string url, CancellationToken token = default);
    Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(string searchInput, int count, CancellationToken token);
    Task<List<VideoModel>> GetPlayListVideosAsync(string url, CancellationToken token = default);
    Task<VideoModel> GetVideoAsync(string url, CancellationToken token = default);
    Task<List<VideoModel>> GetVideosBySearchAsync(string searchInput, int count, CancellationToken token);
}