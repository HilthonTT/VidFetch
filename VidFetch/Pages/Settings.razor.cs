using VidFetch.Models;
using VidFetchLibrary.Library;
using MudBlazor;

namespace VidFetch.Pages;

public partial class Settings
{
    private SaveSettingsModel model = new();
    private string errorMessage = "";

    protected override void OnInitialized()
    {
        MapSettings();
    }

    private void MapSettings()
    {
        model.IsDarkMode = settingsLibrary.IsDarkMode;
        model.DownloadSubtitles = settingsLibrary.DownloadSubtitles;
        model.SaveVideos = settingsLibrary.SaveVideos;
    }

    private async Task SaveSettings()
    {
        try
        {
            errorMessage = "";
            var s = new SettingsLibrary
            {
                IsDarkMode = model.IsDarkMode,
                DownloadSubtitles = model.DownloadSubtitles,
                SaveVideos = model.SaveVideos,
            };
            await settingsData.UpdateSettingsAsync(s);

            navManager.NavigateTo("/Settings", IsReload());

            settingsLibrary.IsDarkMode = model.IsDarkMode;
            settingsLibrary.DownloadSubtitles = model.DownloadSubtitles;
            settingsLibrary.SaveVideos = model.SaveVideos;

            snackbar.Add("Successfully saved settings.", Severity.Normal);
        }
        catch
        {
            errorMessage = "Failed to save settings.";
        }
    }

    private bool IsReload()
    {
        if (settingsLibrary.IsDarkMode == model.IsDarkMode)
        {
            return false;
        }

        return true;
    }
}