using MudBlazor;
using VidFetch.Models;
using VidFetchLibrary.Library;

namespace VidFetch.Pages;

public partial class Settings
{
    private readonly SaveSettingsModel _settingsModel = new();
    private readonly SaveFfmpegSettingsModel _ffmpegSettingsModel = new();
    private SettingsLibrary _settings;
    private List<string> _paths = new();
    private List<string> _formats = new();
    private List<string> _resolutions = new();

    protected override async Task OnInitializedAsync()
    {
        LoadDefaultData();
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        _settings = await settingsData.GetSettingsAsync();
        if (_settings is not null)
        {
            _settingsModel.IsDarkMode = _settings.IsDarkMode;
            _settingsModel.DownloadSubtitles = _settings.DownloadSubtitles;
            _settingsModel.SaveVideos = _settings.SaveVideos;
            _settingsModel.SelectedPath = _settings.SelectedPath;
            _settingsModel.SelectedFormat = _settings.SelectedFormat;
            _ffmpegSettingsModel.SelectedResolution = _settings.SelectedResolution;
            _ffmpegSettingsModel.FfmpegPath = _settings.FfmpegPath;
        }
        else
        {
            _settingsModel.IsDarkMode = true;
            _settingsModel.DownloadSubtitles = false;
            _settingsModel.SaveVideos = false;
            _settingsModel.SelectedFormat = defaultData.GetVideoExtensions().First();
            _settingsModel.SelectedPath = defaultData.GetDownloadPaths().First();
            _ffmpegSettingsModel.SelectedResolution = defaultData.GetVideoResolutions().First();
            _ffmpegSettingsModel.FfmpegPath = "";
        }
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
            bool isReload = _settingsModel.IsDarkMode != settingsLibrary.IsDarkMode;

            _settings.IsDarkMode = _settingsModel.IsDarkMode;
            _settings.DownloadSubtitles = _settingsModel.DownloadSubtitles;
            _settings.SaveVideos = _settingsModel.SaveVideos;
            _settings.SelectedPath = _settingsModel.SelectedPath;
            _settings.SelectedFormat = _settingsModel.SelectedFormat;

            int exitCode = await settingsData.UpdateSettingsAsync(_settings);

            if (exitCode == 1)
            {
                navManager.NavigateTo("/Settings", isReload);
            }
        }
        catch
        {
            snackbar.Add("Failed to save settings.", Severity.Error);
        }
    }


    private async Task SaveFfmpegSettings()
    {
        try
        {
            if (IsValidPath() is false)
            {
                snackbar.Add("Your ffmpeg path doesn't exist.", Severity.Error);
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

            settingsLibrary.SelectedResolution = s.SelectedResolution;
            settingsLibrary.FfmpegPath = s.FfmpegPath;

            snackbar.Add("Successfully saved settings.", Severity.Normal);
        }
        catch
        {
            snackbar.Add("Failed to save settings.", Severity.Error);
        }
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