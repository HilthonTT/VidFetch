using MudBlazor;
using VidFetch.Models;
using VidFetchLibrary.Library;

namespace VidFetch.Pages;

public partial class Settings
{
    private readonly SaveSettingsModel _settingsModel = new();
    private readonly SaveFfmpegSettingsModel _ffmpegSettingsModel = new();
    private List<string> _paths = new();
    private List<string> _formats = new();
    private List<string> _resolutions = new();
    private string _errorMessage = "";

    protected override void OnInitialized()
    {
        MapSettings();
        LoadDefaultData();
    }

    private void MapSettings()
    {
        _settingsModel.IsDarkMode = settingsLibrary.IsDarkMode;
        _settingsModel.DownloadSubtitles = settingsLibrary.DownloadSubtitles;
        _settingsModel.SaveVideos = settingsLibrary.SaveVideos;
        _settingsModel.SelectedPath = settingsLibrary.SelectedPath;
        _settingsModel.SelectedFormat = settingsLibrary.SelectedFormat;
        _ffmpegSettingsModel.SelectedResolution = settingsLibrary.SelectedResolution;
        _ffmpegSettingsModel.FfmpegPath = settingsLibrary.FfmpegPath;
    }

    private void LoadDefaultData()
    {
        _paths = defaultData.GetDownloadPaths();
        _formats = defaultData.GetVideoExtensions();
        _resolutions = defaultData.GetVideoResolutions();
    }

    private async Task SaveAppSettings()
    {
        try
        {
            _errorMessage = "";
            var s = new SettingsLibrary
            {
                Id = settingsLibrary.Id,
                IsDarkMode = _settingsModel.IsDarkMode,
                DownloadSubtitles = _settingsModel.DownloadSubtitles,
                SaveVideos = _settingsModel.SaveVideos,
                SelectedPath = _settingsModel.SelectedPath,
                SelectedFormat = _settingsModel.SelectedFormat,
                SelectedResolution = settingsLibrary.SelectedResolution,
                FfmpegPath = settingsLibrary.FfmpegPath,
            };

            await settingsData.UpdateSettingsAsync(s);

            navManager.NavigateTo("/Settings", IsReload());

            settingsLibrary.IsDarkMode = _settingsModel.IsDarkMode;
            settingsLibrary.DownloadSubtitles = _settingsModel.DownloadSubtitles;
            settingsLibrary.SaveVideos = _settingsModel.SaveVideos;
            settingsLibrary.SelectedPath = _settingsModel.SelectedPath;
            settingsLibrary.SelectedFormat = _settingsModel.SelectedFormat;

            snackbar.Add("Successfully saved settings.", Severity.Normal);
        }
        catch
        {
            _errorMessage = "Failed to save settings.";
        }
    }

    private async Task SaveFfmpegSettings()
    {
        try
        {
            _errorMessage = "";

            if (IsValidPath() is false)
            {
                _errorMessage = "Your ffmpeg path doesn't exist.";
                _ffmpegSettingsModel.FfmpegPath = "";
                return;
            }

            var s = new SettingsLibrary
            {
                Id = settingsLibrary.Id,
                IsDarkMode = settingsLibrary.IsDarkMode,
                DownloadSubtitles = settingsLibrary.DownloadSubtitles,
                SaveVideos = settingsLibrary.SaveVideos,
                SelectedPath = settingsLibrary.SelectedPath,
                SelectedFormat = settingsLibrary.SelectedFormat,
                FfmpegPath = _ffmpegSettingsModel.FfmpegPath,
                SelectedResolution = _ffmpegSettingsModel.SelectedResolution,
            };

            await settingsData.UpdateSettingsAsync(s);

            settingsLibrary.FfmpegPath = _ffmpegSettingsModel.FfmpegPath;
            settingsLibrary.SelectedResolution = _ffmpegSettingsModel.SelectedResolution;

            snackbar.Add("Successfully saved settings.", Severity.Normal);
        }
        catch
        {
            _errorMessage = "Failed to save settings.";
        }
    }

    private bool IsReload()
    {
        if (settingsLibrary.IsDarkMode == _settingsModel.IsDarkMode)
        {
            return false;
        }

        return true;
    }

    private bool IsValidPath()
    {
        string path = _ffmpegSettingsModel.FfmpegPath;

        bool fileExists = File.Exists(path);
        bool isExeFile = Path.GetExtension(path)
            .Equals(".exe", StringComparison.OrdinalIgnoreCase);

        bool isffmpeg = Path.GetFileNameWithoutExtension(path)
            .Equals("ffmpeg", StringComparison.OrdinalIgnoreCase);

        if (fileExists && isExeFile && isffmpeg)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}