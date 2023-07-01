using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Client;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public class VideoData : IVideoData
{
    private const string DbName = "Video.db3";
    private const string CacheName = "VideoData";
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;
    private readonly IYoutube _youtube;
    private SQLiteAsyncConnection _db;

    public VideoData(IMemoryCache cache,
                     IYoutube youtube)
    {
        _cache = cache;
        _youtube = youtube;
    }

    private async Task SetUpDb()
    {
        if (_db is null)
        {
            string dbPath = Path
                .Combine(Environment
                .GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), DbName);

            _db = new(dbPath);
            await _db.CreateTableAsync<VideoModel>();
        }
    }

    public async Task<List<VideoModel>> GetAllVideosAsync()
    {
        await SetUpDb();

        var output = await _cache.GetOrCreateAsync(CacheName, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            return await _db.Table<VideoModel>().ToListAsync();
        });

        return output;
    }

    public async Task<VideoModel> GetVideoAsync(string url, string videoId)
    {
        await SetUpDb();
        string key = GetCache(videoId);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            return await _db.Table<VideoModel>().FirstOrDefaultAsync(v => v.VideoId == videoId || v.Url == url);
        });

        return output;
    }

    public async Task<bool> VideoExistsAsync(string url, string videoId)
    {
        await SetUpDb();

        var video = await GetVideoAsync(url, videoId);
        if (video is null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task<int> SetVideoAsync(string url, string videoId)
    {
        await SetUpDb();

        var existingVideo = await GetVideoAsync(url, videoId);

        string key = GetCache(videoId);
        RemoveCache(key);

        if (existingVideo is null)
        {
            var video = await _youtube.GetVideoAsync(url);
            return await CreateVideoAsync(video);
        }
        else
        {
            return await UpdateVideoAsync(existingVideo);
        }
    }

    public async Task<int> DeleteVideoAsync(VideoModel video)
    {
        await SetUpDb();

        string key = GetCache(video.VideoId);
        RemoveCache(key);

        return await _db.DeleteAsync<VideoModel>(video.Id);
    }

    public void RemoveVideoCache(string videoId)
    {
        string key = GetCache(videoId);

        _cache.Remove(key);
    }

    private async Task<int> CreateVideoAsync(VideoModel video)
    {
        var v = await FillDataAsync(video);

        return await _db.InsertAsync(v);
    }

    private async Task<int> UpdateVideoAsync(VideoModel video)
    {
        return await _db.UpdateAsync(video);
    }

    private void RemoveCache(string videoId = "")
    {
        _cache.Remove(CacheName);

        if (string.IsNullOrWhiteSpace(videoId) is false)
        {
            string key = GetCache(videoId);
            _cache.Remove(key);
        }
    }

    private static string GetCache(string id)
    {
        return $"{CacheName}-{id}";
    }

    private async Task<VideoModel> FillDataAsync(VideoModel video)
    {
        string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";

        var channel = await _youtube.GetChannelAsync(video.AuthorUrl);
        string channelThumbnail = string.IsNullOrWhiteSpace(channel.ThumbnailUrl) ? defaultUrl : channel.ThumbnailUrl;

        video.AuthorThumbnailUrl = channelThumbnail;

        return video;
    }
}
