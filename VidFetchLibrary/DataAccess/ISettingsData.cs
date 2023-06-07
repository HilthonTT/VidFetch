using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface ISettingsData
{
    Task<SettingsModel> GetSettingsAsync();
    Task<int> UpdateSettingsAsync(SettingsModel settings);
}