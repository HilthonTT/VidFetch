using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Client;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public class ChannelData : IChannelData
{
    private const string DbName = "Channel.db3";
    private const string CacheName = "ChannelData";
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;
    private readonly IYoutube _youtube;
    private SQLiteAsyncConnection _db;

    public ChannelData(IMemoryCache cache,
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
            await _db.CreateTableAsync<ChannelModel>();
        }
    }

    public async Task<List<ChannelModel>> GetAllChannelsAsync()
    {
        await SetUpDb();

        var output = await _cache.GetOrCreateAsync(CacheName, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            return await _db.Table<ChannelModel>().ToListAsync();
        });
        
        return output;
    }

    public async Task<ChannelModel> GetChannelAsync(string url, string channelId)
    {
        await SetUpDb();
        string key = GetCache(channelId);

        var output = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);
            return await _db.Table<ChannelModel>().FirstOrDefaultAsync(c => c.Url == url || c.ChannelId == channelId);
        });

        return output;
    }

    public async Task<bool> ChannelExistsAsync(string url, string channelId)
    {
        await SetUpDb();

        var channel = await GetChannelAsync(url, channelId);
        if (channel is null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task<int> SetChannelAsync(string url, string channelId)
    {
        await SetUpDb();

        var existingChannel = await GetChannelAsync(url, channelId);

        string key = GetCache(channelId);
        RemoveCache(key);

        if (existingChannel is null)
        {
            var channel = await _youtube.GetChannelAsync(url);
            return await CreateChannelAsync(channel);
        }
        else
        {
            return await UpdateChannelAsync(existingChannel);
        }
    }

    public async Task<int> DeleteChannelAsync(ChannelModel channel)
    {
        await SetUpDb();

        string key = GetCache(channel.ChannelId);
        RemoveCache(key);

        return await _db.DeleteAsync<ChannelModel>(channel.Id);
    }

    private async Task<int> CreateChannelAsync(ChannelModel channel)
    {
        return await _db.InsertAsync(channel);
    }

    private async Task<int> UpdateChannelAsync(ChannelModel channel)
    {
        return await _db.UpdateAsync(channel);
    }

    private void RemoveCache(string channelId = "")
    {
        _cache.Remove(CacheName);

        if (string.IsNullOrWhiteSpace(channelId) is false)
        {
            string key = GetCache(channelId);
            _cache.Remove(key);
        }
    }

    private static string GetCache(string channelId)
    {
        return $"{CacheName}-{channelId}";
    }
}
