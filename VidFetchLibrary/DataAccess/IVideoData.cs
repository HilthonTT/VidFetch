using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface IVideoData
{
    Task<int> DeleteAsync(VideoModel video);
    Task<List<VideoModel>> GetAllVideosAsync();
    Task<VideoModel> GetVideoAsync(string url, string videoId);
    Task<int> SetVideoAsync(string url, string videoId);
    Task<bool> VideoExistAsync(string url, string videoId);
}