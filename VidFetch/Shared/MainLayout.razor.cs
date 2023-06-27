using VidFetchLibrary.Library;

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
            MapSettings();
        }
        else
        {
            _isDarkMode = true;

            settingsLibrary.IsDarkMode = true;
            settingsLibrary.DownloadSubtitles = false;
            settingsLibrary.SaveVideos = false;
            settingsLibrary.SelectedFormat = defaultData.GetVideoExtensions().First();
            settingsLibrary.SelectedPath = defaultData.GetDownloadPaths().First();
            settingsLibrary.SelectedResolution = defaultData.GetVideoResolutions().First();
            settingsLibrary.FfmpegPath = "";
        }
    }

    private void MapSettings()
    {
        settingsLibrary.Id = _settings.Id;
        settingsLibrary.IsDarkMode = _settings.IsDarkMode;
        settingsLibrary.DownloadSubtitles = _settings.DownloadSubtitles;
        settingsLibrary.SaveVideos = _settings.SaveVideos;
        settingsLibrary.SelectedFormat = _settings.SelectedFormat;
        settingsLibrary.SelectedPath = _settings.SelectedPath;
        settingsLibrary.SelectedResolution = _settings.SelectedResolution;
        settingsLibrary.FfmpegPath = _settings.FfmpegPath;
    }
}