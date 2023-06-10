using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public class VideoData : IVideoData
{
    private const string DbName = "Video.db3";
    private const string CacheName = "VideoData";
    private readonly IMemoryCache _cache;
    private SQLiteAsyncConnection _db;

    public VideoData(IMemoryCache cache)
    {
        _cache = cache;
        SetUpDb();
    }

    private void SetUpDb()
    {
        if (_db is null)
        {
            string dbPath = Path
                .Combine(Environment
                .GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), DbName);

            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<VideoModel>();
        }
    }

    public async Task<List<VideoModel>> GetAllVideosAsync()
    {
        var output = _cache.Get<List<VideoModel>>(CacheName);
        if (output is null)
        {
            output = await _db.Table<VideoModel>().ToListAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<int> AddVideoAsync(VideoModel video)
    {
        RemoveCache();
        return await _db.InsertAsync(video);
    }

    public async Task<int> AddVideosAsync(List<VideoModel> videos)
    {
        RemoveCache();
        return await _db.InsertAllAsync(videos);
    }

    public async Task<int> UpdateVideoAsync(VideoModel video)
    {
        RemoveCache();
        return await _db.UpdateAsync(video);
    }

    public async Task<int> DeleteVideoAsync(VideoModel video)
    {
        RemoveCache();
        return await _db.DeleteAsync(video);
    }

    private void RemoveCache()
    {
        _cache.Remove(CacheName);
    }
}
