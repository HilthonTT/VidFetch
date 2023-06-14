using VidFetchLibrary.Library;

namespace VidFetch.Shared;

public partial class MainLayout
{
    private SettingsLibrary settings;
    private bool isDarkMode = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        settings = await settingsData.GetSettingsAsync();

        if (settings is not null)
        {
            isDarkMode = settings.IsDarkMode;
            MapSettings();
        }
        else
        {
            isDarkMode = true;

            settingsLibrary.IsDarkMode = true;
            settingsLibrary.DownloadSubtitles = false;
            settingsLibrary.SaveVideos = false;
        }
    }

    private void MapSettings()
    {
        settingsLibrary.Id = settings.Id;
        settingsLibrary.IsDarkMode = settings.IsDarkMode;
        settingsLibrary.DownloadSubtitles = settings.DownloadSubtitles;
        settingsLibrary.SaveVideos = settings.SaveVideos;
    }
}