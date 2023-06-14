using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface IVideoData
{
    Task<int> DeleteAsync(VideoModel video);
    Task<List<VideoModel>> GetAllVideosAsync();
    Task<VideoModel> GetVideoAsync(int id);
    Task<int> SetVideoAsync(string url, string videoId);
}