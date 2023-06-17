using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Client;
using VidFetchLibrary.Models;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.DataAccess;
public class VideoData : IVideoData
{
    private const string DbName = "Video.db3";
    private const string CacheName = "VideoData";
    private readonly IMemoryCache _cache;
    private readonly IYoutube _youtube;
    private SQLiteAsyncConnection _db;

    public VideoData(IMemoryCache cache,
                     IYoutube youtube)
    {
        _cache = cache;
        _youtube = youtube;
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

            _db = new(dbPath);
            _db.CreateTableAsync<VideoModel>();
        }
    }

    public async Task<List<VideoModel>> GetAllVideosAsync()
    {
        var output = _cache.Get<List<VideoModel>>(CacheName);
        if (output is null)
        {
            output = await _db.Table<VideoModel>().ToListAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<VideoModel> GetVideoAsync(string url, string videoId)
    {
        string key = GetCache(videoId);

        var output = _cache.Get<VideoModel>(key);
        if (output is null)
        {
            output = await _db.Table<VideoModel>().FirstOrDefaultAsync(v => v.VideoId == videoId || v.Url == url);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<bool> VideoExistAsync(string url, string videoId)
    {
        var video = await GetVideoAsync(url, videoId);
        if (video is null)
        {
            return false;
        }

        return true;
    }

    public async Task<int> SetVideoAsync(string url, string videoId)
    {
        var existingVideo = await _db.Table<VideoModel>().FirstOrDefaultAsync(
            v => v.Url == url || v.VideoId == videoId
        );
        
        RemoveCache();

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

    public async Task<int> DeleteAsync(VideoModel video)
    {
        string key = GetCache(video.VideoId);

        var existingVideo = await _db.Table<VideoModel>().FirstOrDefaultAsync(
            v => v.Id == video.Id || v.Url == video.Url
        );

        if (existingVideo is not null)
        {
            RemoveCache(key);
            return await _db.DeleteAsync<VideoModel>(existingVideo.Id);
        }
        else
        {
            return 0;
        }
    }

    private async Task<int> CreateVideoAsync(Video video) // Using YoutubeExplode Video Class to create 
    {
        var v = await MapVideoAsync(video);

        return await _db.InsertAsync(v);
    }

    private async Task<int> UpdateVideoAsync(VideoModel video)
    {
        return await _db.UpdateAsync(video);
    }

    private void RemoveCache(string id = "")
    {
        _cache.Remove(CacheName);

        if (string.IsNullOrWhiteSpace(id) is false)
        {
            _cache.Remove(id);
        }
    }

    private static string GetCache(string id)
    {
        return $"{CacheName}-{id}";
    }

    private async Task<VideoModel> MapVideoAsync(Video video)
    {
        string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";
        string thumbnailUrl = video.Thumbnails.Count > 0 ? video.Thumbnails[0].Url : defaultUrl;

        var channel = await _youtube.GetChannelAsync(video.Author.ChannelUrl);
        string channelThumbnail = channel.Thumbnails.Count > 0 ? channel.Thumbnails[0].Url : defaultUrl;

        return new VideoModel()
        {
            Title = video.Title,
            VideoId = video.Id,
            Description = video.Description,
            Url = video.Url,
            AuthorName = video.Author.ChannelTitle,
            AuthorUrl = video.Author.ChannelUrl,
            AuthorThumbnailUrl = channelThumbnail,
            ThumbnailUrl = thumbnailUrl,
            Keywords = video.Keywords.ToList(),
            Duration = video.Duration.Value,
            UploadDate = video.UploadDate,
        };
    }
}
