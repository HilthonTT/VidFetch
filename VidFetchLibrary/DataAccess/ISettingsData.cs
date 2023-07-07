using VidFetchLibrary.Library;

namespace VidFetchLibrary.DataAccess;
public interface ISettingsData
{
    SettingsLibrary GetSettings();
    Task<SettingsLibrary> GetSettingsAsync();
    Task<int> SetSettingsAsync(SettingsLibrary settings);
}