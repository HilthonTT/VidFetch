using VidFetch.Models;
using VidFetchLibrary.Library;
using MudBlazor;

namespace VidFetch.Pages;

public partial class Settings
{
    private readonly SaveSettingsModel _model = new();
    private List<string> _paths = new();
    private List<string> _formats = new();
    private string _errorMessage = "";

    protected override void OnInitialized()
    {
        MapSettings();
        LoadPathsAndFormats();
    }

    private void MapSettings()
    {
        _model.IsDarkMode = settingsLibrary.IsDarkMode;
        _model.DownloadSubtitles = settingsLibrary.DownloadSubtitles;
        _model.SaveVideos = settingsLibrary.SaveVideos;
        _model.SelectedPath = settingsLibrary.SelectedPath;
        _model.SelectedFormat = settingsLibrary.SelectedFormat;
    }

    private void LoadPathsAndFormats()
    {
        _paths = defaultData.GetDownloadPaths();
        _formats = defaultData.GetVideoExtensions();
    }

    private async Task SaveSettings()
    {
        try
        {
            _errorMessage = "";
            var s = new SettingsLibrary
            {
                IsDarkMode = _model.IsDarkMode,
                DownloadSubtitles = _model.DownloadSubtitles,
                SaveVideos = _model.SaveVideos,
                SelectedPath = _model.SelectedPath,
                SelectedFormat = _model.SelectedFormat,
            };

            await settingsData.UpdateSettingsAsync(s);

            navManager.NavigateTo("/Settings", IsReload());

            settingsLibrary.IsDarkMode = _model.IsDarkMode;
            settingsLibrary.DownloadSubtitles = _model.DownloadSubtitles;
            settingsLibrary.SaveVideos = _model.SaveVideos;
            settingsLibrary.SelectedPath = _model.SelectedPath;
            settingsLibrary.SelectedFormat = _model.SelectedFormat;

            snackbar.Add("Successfully saved settings.", Severity.Normal);
        }
        catch
        {
            _errorMessage = "Failed to save settings.";
        }
    }

    private bool IsReload()
    {
        if (settingsLibrary.IsDarkMode == _model.IsDarkMode)
        {
            return false;
        }

        return true;
    }
}