using MudBlazor;
using VidFetch.Models;
using VidFetchLibrary.Library;
using VidFetchLibrary.Data;

namespace VidFetch.Pages;

public partial class Settings
{
    private readonly SaveSettingsModel _settingsModel = new();
    private readonly SaveFfmpegSettingsModel _ffmpegSettingsModel = new();
    private SettingsLibrary _settings;
    private List<DownloadPath> _paths = new();
    private List<VideoExtension> _formats = new();
    private List<VideoResolution> _resolutions = new();

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
            _settingsModel.SelectedResolution = _settings.SelectedResolution;
            _settingsModel.CreateSubDirectoryPlaylist = _settings.CreateSubDirectoryPlaylist;
            _ffmpegSettingsModel.SelectedResolution = _settings.SelectedResolution;
            _ffmpegSettingsModel.FfmpegPath = _settings.FfmpegPath;
        }
        else
        {
            _settingsModel.IsDarkMode = true;
            _settingsModel.DownloadSubtitles = false;
            _settingsModel.SaveVideos = false;
            _settingsModel.CreateSubDirectoryPlaylist = false;
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

            var s = new SettingsLibrary
            {
                IsDarkMode = _settingsModel.IsDarkMode,
                DownloadSubtitles = _settingsModel.DownloadSubtitles,
                SaveVideos = _settingsModel.SaveVideos,
                SelectedPath = _settingsModel.SelectedPath,
                SelectedFormat = _settingsModel.SelectedFormat,
                SelectedResolution = _settingsModel.SelectedResolution,
                CreateSubDirectoryPlaylist = _settingsModel.CreateSubDirectoryPlaylist,
            };

            _ffmpegSettingsModel.SelectedResolution = s.SelectedResolution;

            int exitCode = await settingsData.UpdateSettingsAsync(s);

            if (exitCode == 1)
            {
                navManager.NavigateTo("/Settings", isReload);
            }

            snackbar.Add("Successfully saved settings.");
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

            _settingsModel.SelectedResolution = _ffmpegSettingsModel.SelectedResolution;

            settingsLibrary.SelectedResolution = s.SelectedResolution;
            settingsLibrary.FfmpegPath = s.FfmpegPath;

            snackbar.Add("Successfully saved settings.");
        }
        catch
        {
            snackbar.Add("Failed to save settings.", Severity.Error);
        }
    }

    private string GetSpacedString(string path)
    {
        return pathHelper.GetSpacedString(path);
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