using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Library;

namespace VidFetchLibrary.DataAccess;
public class SettingsData : ISettingsData
{
    private const string DbName = "Settings.db3";
    private const string CacheName = "SettingsData";
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;
    private SQLiteAsyncConnection _asyncDbConnection;
    private SQLiteConnection _dbConnection;

    public SettingsData(IMemoryCache cache)
    {
        _cache = cache;
        SetUpDb();
    }

    private void SetUpDb()
    {
        if (_dbConnection is null)
        {
            string dbPath = Path
                .Combine(Environment
                .GetFolderPath(
                    Environment.SpecialFolder
                    .LocalApplicationData), DbName);

            _dbConnection = new(dbPath);
            _dbConnection.CreateTable<SettingsLibrary>();
        }
    }

    private async Task SetUpDbAsync()
    {
        if (_asyncDbConnection is null)
        {
            string dbPath = Path
                .Combine(Environment
                .GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), DbName);

            _asyncDbConnection = new(dbPath);
            await _asyncDbConnection.CreateTableAsync<SettingsLibrary>();
        }
    }

    public async Task<SettingsLibrary> GetSettingsAsync()
    {
        await SetUpDbAsync();

        var output = await _cache.GetOrCreateAsync(CacheName, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            return await _asyncDbConnection.Table<SettingsLibrary>()
                .FirstOrDefaultAsync();
        });

        if (output is null)
        {
            _cache.Remove(CacheName);
            return new();
        }

        return output;
    }

    public async Task<int> SetSettingsAsync(SettingsLibrary settings)
    {        
        var existingSettings = await GetSettingsAsync();
        int exitCode;

        if (existingSettings is not null)
        {
            settings.Id = existingSettings.Id;
            exitCode = await _asyncDbConnection.UpdateAsync(settings);
        }
        else
        {
            exitCode = await _asyncDbConnection.InsertAsync(settings);
        }

        _cache.Remove(CacheName);

        return exitCode;
    }

    public SettingsLibrary GetSettings()
    {
        var output = _cache.GetOrCreate(CacheName, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            return _dbConnection.Table<SettingsLibrary>().FirstOrDefault();
        });

        if (output is null)
        {
            _cache.Remove(CacheName);
            return new();
        }

        return output;
    }
}
