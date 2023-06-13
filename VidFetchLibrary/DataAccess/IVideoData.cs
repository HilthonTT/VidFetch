using VidFetchLibrary.Models;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.DataAccess;
public interface IVideoData
{
    Task<int> DeleteAsync(VideoModel video);
    Task<List<VideoModel>> GetAllVideosAsync();
    Task<VideoModel> GetVideoAsync(int id);
    Task<int> SetAsync(Video video);
}