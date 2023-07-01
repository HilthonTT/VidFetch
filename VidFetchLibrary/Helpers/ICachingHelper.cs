using VidFetchLibrary.Models;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Helpers;
public interface ICachingHelper
{
    string CachePrimaryVideoKey(string url);
    string CacheSecondaryVideoKey(string url);
    string CacheStreamManifest(string id);
    string CacheSubtitlesInfoKey(string id);
    Task<ChannelModel> GetChannelAsync(string url);
    Task<List<ChannelModel>> GetChannelBySearchAsync(string searchInput, CancellationToken token);
    Task<List<VideoModel>> GetChannelVideosAsync(string url);
    Task<PlaylistModel> GetPlaylistAsync(string url);
    Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(string searchInput, CancellationToken token);
    Task<List<VideoModel>> GetPlayListVideosAsync(string url);
    Task<VideoModel> GetVideoAsync(string url);
    Task<List<VideoModel>> GetVideosBySearchAsync(string searchInput, CancellationToken token);
    Task<Video> LoadYoutubeExplodeVideoAsync(string url, CancellationToken token);
}