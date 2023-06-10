using VidFetchLibrary.Library;

namespace VidFetchLibrary.DataAccess;
public interface ISettingsData
{
    Task<SettingsLibrary> GetSettingsAsync();
    Task<int> UpdateSettingsAsync(SettingsLibrary settings);
}