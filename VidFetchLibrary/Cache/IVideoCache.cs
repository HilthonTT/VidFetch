using VidFetchLibrary.Models;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Cache;
public interface IVideoCache
{
    string CachePrimaryVideoKey(string url);
    string CacheSecondaryVideoKey(string url);
    Task<List<VideoModel>> GetChannelVideosAsync(string url, CancellationToken token = default);
    Task<List<VideoModel>> GetPlayListVideosAsync(string url, CancellationToken token = default);
    Task<VideoModel> GetVideoAsync(string url, CancellationToken token = default);
    Task<List<VideoModel>> GetVideosBySearchAsync(string searchInput, CancellationToken token = default);
    Task<Video> LoadYoutubeExplodeVideoAsync(string url, CancellationToken token = default);
}