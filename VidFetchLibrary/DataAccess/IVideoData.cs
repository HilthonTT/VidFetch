using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface IVideoData
{
    Task<int> AddVideoAsync(VideoModel video);
    Task<int> AddVideosAsync(List<VideoModel> videos);
    Task<int> DeleteVideoAsync(VideoModel video);
    Task<List<VideoModel>> GetAllVideosAsync();
    Task<int> UpdateVideoAsync(VideoModel video);
}