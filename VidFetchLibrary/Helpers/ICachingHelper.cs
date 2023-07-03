using VidFetchLibrary.Models;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Helpers;
public interface ICachingHelper
{
    string CachePrimaryVideoKey(string url);
    string CacheSecondaryVideoKey(string url);
    string CacheStreamManifest(string id);
    string CacheSubtitlesInfoKey(string id);
    Task<ChannelModel> GetChannelAsync(string url, CancellationToken token = default);
    Task<List<ChannelModel>> GetChannelBySearchAsync(string searchInput, CancellationToken token = default);
    Task<List<VideoModel>> GetChannelVideosAsync(string url, CancellationToken token = default);
    Task<PlaylistModel> GetPlaylistAsync(string url, CancellationToken token = default);
    Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(string searchInput, CancellationToken token = default);
    Task<List<VideoModel>> GetPlayListVideosAsync(string url, CancellationToken token = default);
    Task<VideoModel> GetVideoAsync(string url, CancellationToken token = default);
    Task<List<VideoModel>> GetVideosBySearchAsync(string searchInput, CancellationToken token = default);
    Task<Video> LoadYoutubeExplodeVideoAsync(string url, CancellationToken token = default);
}