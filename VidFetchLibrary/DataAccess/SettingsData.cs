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

    public async Task<SettingsLibrary> GetSettingsAsync()
    {
        await SetUpDb();

        var output = await _cache.GetOrCreateAsync(CacheName, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            return await _db.Table<SettingsLibrary>()
                .FirstOrDefaultAsync();
        });

        if (output is null)
        {
            _cache.Remove(CacheName);
        }

        return output;
    }

    public async Task<int> SetSettingsAsync(SettingsLibrary settings)
    {        
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
        _settings.RemoveAfterDownload = settings.RemoveAfterDownload;
    }
}
