using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Client;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public class ChannelData : IChannelData
{
    private const string DbName = "Channel.db3";
    private const string CacheName = "ChannelData";
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

        var output = _cache.Get<List<ChannelModel>>(CacheName);
        if (output is null)
        {
            output = await _db.Table<ChannelModel>().ToListAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<ChannelModel> GetChannelAsync(string url, string channelId)
    {
        await SetUpDb();
        string key = GetCache(channelId);

        var output = _cache.Get<ChannelModel>(key);
        if (output is null)
        {
            output = await _db.Table<ChannelModel>().FirstOrDefaultAsync(c => c.Url == url || c.ChannelId == channelId);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

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
        RemoveCache();

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

        var existingChannel = await GetChannelAsync(channel.Url, channel.ChannelId);

        if (existingChannel is null)
        {
            RemoveCache(key);
            return await _db.DeleteAsync<ChannelModel>(channel.Id);
        }
        else
        {
            return 0;
        }
    }

    private async Task<int> CreateChannelAsync(ChannelModel channel)
    {
        return await _db.InsertAsync(channel);
    }

    private async Task<int> UpdateChannelAsync(ChannelModel channel)
    {
        return await _db.UpdateAsync(channel);
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
}
