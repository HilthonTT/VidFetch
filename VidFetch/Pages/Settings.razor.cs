using VidFetch.Models;
using VidFetchLibrary.Library;
using MudBlazor;

namespace VidFetch.Pages;

public partial class Settings
{
    private SaveSettingsModel model = new();
    private List<string> paths = new();
    private List<string> formats = new();
    private string errorMessage = "";

    protected override void OnInitialized()
    {
        MapSettings();
        LoadPathsAndFormats();
    }

    private void MapSettings()
    {
        model.IsDarkMode = settingsLibrary.IsDarkMode;
        model.DownloadSubtitles = settingsLibrary.DownloadSubtitles;
        model.SaveVideos = settingsLibrary.SaveVideos;
        model.SelectedPath = settingsLibrary.SelectedPath;
        model.SelectedFormat = settingsLibrary.SelectedFormat;
    }

    private void LoadPathsAndFormats()
    {
        paths = defaultData.GetDownloadPaths();
        formats = defaultData.GetVideoExtensions();
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
                SelectedPath = model.SelectedPath,
                SelectedFormat = model.SelectedFormat,
            };

            await settingsData.UpdateSettingsAsync(s);

            navManager.NavigateTo("/Settings", IsReload());

            settingsLibrary.IsDarkMode = model.IsDarkMode;
            settingsLibrary.DownloadSubtitles = model.DownloadSubtitles;
            settingsLibrary.SaveVideos = model.SaveVideos;
            settingsLibrary.SelectedPath = model.SelectedPath;
            settingsLibrary.SelectedFormat = model.SelectedFormat;

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