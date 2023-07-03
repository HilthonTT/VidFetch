using VidFetchLibrary.Library;
using VidFetchLibrary.Data;

namespace VidFetch.Shared;

public partial class MainLayout
{
    private SettingsLibrary _settings;
    private bool _isDarkMode = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        _settings = await settingsData.GetSettingsAsync();

        if (_settings is not null)
        {
            _isDarkMode = _settings.IsDarkMode;
            settingsLibrary = new SettingsLibrary(_settings);
        }
        else
        {
            settingsLibrary = new SettingsLibrary
            {
                SelectedPath = DownloadPath.DownloadFolder,
                SelectedFormat = VideoExtension.Mp4,
                SelectedResolution = VideoResolution.P1080,
            };
        }
    }
}