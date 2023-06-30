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
    private readonly ISettingsLibrary _settings;
    private SQLiteAsyncConnection _db;

    public SettingsData(IMemoryCache cache,
                        ISettingsLibrary settings)
    {
        _cache = cache;
        _settings = settings;
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

    /// <summary>
    /// Kept the old way because of it being an issue with reloading the page.
    /// </summary>
    /// <returns>Returns SettingsLibrary</returns>
    public async Task<SettingsLibrary> GetSettingsAsync()
    {
        await SetUpDb();

        var output = _cache.Get<SettingsLibrary>(CacheName);
        if (output is null)
        {
            output = await _db.Table<SettingsLibrary>().FirstOrDefaultAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(CacheTime));
        }

        return output;
    }

    public async Task<int> UpdateSettingsAsync(SettingsLibrary settings)
    {
        await SetUpDb();
        
        var existingSettings = await GetSettingsAsync();
        if (existingSettings is not null)
        {
            settings.Id = existingSettings.Id;
            MapSettingsLibrary(settings);
            return await _db.UpdateAsync(settings);
        }
        else
        {
            MapSettingsLibrary(settings);
            return await _db.InsertAsync(settings);
        }
    }

    private void MapSettingsLibrary(SettingsLibrary settings)
    {
        _cache.Remove(CacheName);
        _settings.Id = settings.Id;
        _settings.IsDarkMode = settings.IsDarkMode;
        _settings.DownloadSubtitles = settings.DownloadSubtitles;
        _settings.SaveVideos = settings.SaveVideos;
        _settings.SelectedPath = settings.SelectedPath;
        _settings.SelectedFormat = settings.SelectedFormat;
        _settings.SelectedResolution = settings.SelectedResolution;
        _settings.FfmpegPath = settings.FfmpegPath;
        _settings.CreateSubDirectoryPlaylist = settings.CreateSubDirectoryPlaylist;
    }
}
