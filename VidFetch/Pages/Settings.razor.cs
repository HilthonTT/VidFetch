using MudBlazor;
using VidFetch.Models;
using VidFetchLibrary.Library;
using VidFetchLibrary.Data;
using VidFetchLibrary.Language;
using System.Globalization;

namespace VidFetch.Pages;

public partial class Settings
{
    private SaveSettingsModel _settingsModel = new();
    private SaveFfmpegSettingsModel _ffmpegSettingsModel = new();
    private SettingsLibrary _settings;
    private List<DownloadPath> _paths = new();
    private List<VideoExtension> _formats = new();
    private List<VideoResolution> _resolutions = new();
    private List<Language> _languages = new();

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
            _settingsModel = new(_settings);
            _ffmpegSettingsModel = new(_settings);
        }
    }

    private void LoadDefaultData()
    {
        _paths = defaultData.GetDownloadPaths();
        _formats = defaultData.GetVideoExtensions();
        _resolutions = defaultData.GetVideoResolutions();
        _languages = defaultData.GetLanguages();
    }

    private async Task SaveAppSettings()
    {
        try
        {
            bool shouldRender = ShouldReloadSettings();

            _settings.IsDarkMode = _settingsModel.IsDarkMode;
            _settings.DownloadSubtitles = _settingsModel.DownloadSubtitles;
            _settings.SelectedPath = _settingsModel.SelectedPath;
            _settings.SaveVideos = _settingsModel.SaveVideos;
            _settings.SelectedFormat = _settingsModel.SelectedFormat;
            _settings.CreateSubDirectoryPlaylist = _settingsModel.CreateSubDirectoryPlaylist;
            _settings.RemoveAfterDownload = _settingsModel.RemoveAfterDownload;
            _settings.SelectedResolution = _settingsModel.SelectedResolution;
            _settings.SelectedLanguage = _settingsModel.SelectedLanguage;

            _ffmpegSettingsModel.SelectedResolution = _settings.SelectedResolution;

            int exitCode = await settingsData.SetSettingsAsync(_settings);

            if (exitCode is 1)
            {
                navManager.NavigateTo("/Settings", shouldRender);
            }

            string successMessage = GetDictionary()[KeyWords.SuccessfullySettings];
            snackbar.Add(successMessage);
        }
        catch
        {
            string errorMessage = GetDictionary()[KeyWords.FailedSettings];
            snackbar.Add(errorMessage, Severity.Error);
        }
    }

    private async Task SaveFfmpegSettings()
    {
        try
        {
            if (IsValidPath() is false)
            {
                string errorMessage = GetDictionary()[KeyWords.FfmpathNotExistError];
                snackbar.Add(errorMessage, Severity.Error);
                
                _ffmpegSettingsModel.FfmpegPath = "";
                return;
            }

            _settings.FfmpegPath = _ffmpegSettingsModel.FfmpegPath;
            _settings.SelectedResolution = _ffmpegSettingsModel.SelectedResolution;

            await settingsData.SetSettingsAsync(_settings);

            _settingsModel.SelectedResolution = _ffmpegSettingsModel.SelectedResolution;

            string successMessage = GetDictionary()[KeyWords.SuccessfullySettings];
            snackbar.Add(successMessage);
        }
        catch
        {
            string errorMessage = GetDictionary()[KeyWords.FailedSettings];
            snackbar.Add(errorMessage, Severity.Error);
        }
    }

    private async Task ClearFfmpegPath()
    {
        _ffmpegSettingsModel.FfmpegPath = "";

        var settings = await settingsData.GetSettingsAsync();
        settings.FfmpegPath = "";

        await settingsData.SetSettingsAsync(settings);

        string successText = GetDictionary()[KeyWords.FfmpathCleared];
        snackbar.Add(successText);
    }

    private string GetSpacedString(string path)
    {
        return pathHelper.GetSpacedString(path);
    }

    private Dictionary<KeyWords, string> GetDictionary()
    {
        var dictionary = languageExtension.GetDictionary();
        return dictionary;
    }

    private static CultureInfo GetCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        return culture;
    }

    private string GetLanguage(Language language)
    {
        return language switch
        {
            Language.English => GetDictionary()[KeyWords.English],
            Language.French => GetDictionary()[KeyWords.French],
            Language.Indonesian => GetDictionary()[KeyWords.Indonesian],
            _ => "",
        };
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

    private bool ShouldReloadSettings()
    {
        if (_settingsModel.IsDarkMode != _settings.IsDarkMode || 
            _settingsModel.SelectedLanguage != _settings.SelectedLanguage)
        {
            return true;
        }

        return false;
    }

}