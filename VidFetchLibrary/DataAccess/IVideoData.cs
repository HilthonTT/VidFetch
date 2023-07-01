using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface IVideoData
{
    Task<int> DeleteVideoAsync(VideoModel video);
    Task<List<VideoModel>> GetAllVideosAsync();
    Task<VideoModel> GetVideoAsync(string url, string videoId);
    void RemoveVideoCache(string id);
    Task<int> SetVideoAsync(string url, string videoId);
    Task<bool> VideoExistsAsync(string url, string videoId);
}