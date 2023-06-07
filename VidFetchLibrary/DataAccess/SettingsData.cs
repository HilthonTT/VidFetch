using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public class SettingsData : ISettingsData
{
    private const string DbName = "Settings.db3";
    private const string CacheName = "SettingsData";
    private readonly IMemoryCache _cache;
    private SQLiteAsyncConnection _db;

    public SettingsData(IMemoryCache cache)
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

            _db = new(dbPath);
            _db.CreateTableAsync<SettingsModel>();
        }
    }

    public async Task<SettingsModel> GetSettingsAsync()
    {
        var output = _cache.Get<SettingsModel>(CacheName);
        if (output is null)
        {
            output = await _db.Table<SettingsModel>().FirstOrDefaultAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<int> UpdateSettingsAsync(SettingsModel settings)
    {
        RemoveCache();
        if (settings.Id == 0)
        {
            return await _db.InsertAsync(settings);
        }
        else
        {
            return await _db.UpdateAsync(settings);
        }
    }

    private void RemoveCache()
    {
        _cache.Remove(CacheName);
    }
}
