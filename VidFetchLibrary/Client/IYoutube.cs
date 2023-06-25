using VidFetchLibrary.Models;

namespace VidFetchLibrary.Client;

public interface IYoutube
{
    Task DownloadVideoAsync(string url, IProgress<double> progress, CancellationToken token);
    Task<ChannelModel> GetChannelAsync(string url);
    Task<List<ChannelModel>> GetChannelBySearchAsync(string searchInput, CancellationToken token);
    Task<List<VideoModel>> GetChannelVideosAsync(string url);
    Task<PlaylistModel> GetPlaylistAsync(string url);
    Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(string searchInput, CancellationToken token);
    Task<List<VideoModel>> GetPlayListVideosAsync(string url);
    Task<VideoModel> GetVideoAsync(string url);
    Task<List<VideoModel>> GetVideosBySearchAsync(string searchInput, CancellationToken token);
}