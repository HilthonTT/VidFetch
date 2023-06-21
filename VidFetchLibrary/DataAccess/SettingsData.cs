using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Library;

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
            await _db.CreateTableAsync<SettingsLibrary>();
        }
    }

    public async Task<SettingsLibrary> GetSettingsAsync()
    {
        await SetUpDb();

        var output = _cache.Get<SettingsLibrary>(CacheName);
        if (output is null)
        {
            output = await _db.Table<SettingsLibrary>().FirstOrDefaultAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(1));
        }

        return output;
    }

    public async Task<int> UpdateSettingsAsync(SettingsLibrary settings)
    {
        await SetUpDb();

        RemoveCache();
        var existingSettings = await _db.Table<SettingsLibrary>().FirstOrDefaultAsync();
        if (existingSettings is not null)
        {
            settings.Id = existingSettings.Id;
            return await _db.UpdateAsync(settings);
        }
        else
        {
            return await _db.InsertAsync(settings);
        }
    }

    private void RemoveCache()
    {
        _cache.Remove(CacheName);
    }
}
