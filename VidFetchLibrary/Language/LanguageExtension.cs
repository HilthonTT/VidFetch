using VidFetchLibrary.DataAccess;

namespace VidFetchLibrary.Language;
public class LanguageExtension : ILanguageExtension
{
    private readonly ISettingsData _settingsData;

    public LanguageExtension(ISettingsData settingsData)
    {
        _settingsData = settingsData;
    }

    public Dictionary<KeyWords, string> GetDictionary(string text = "")
    {
        var settings = _settingsData.GetSettings();

        return settings.SelectedLanguage switch
        {
            Data.Language.English => EnglishDictionary(text),
            Data.Language.French => FrenchDictionary(text),
            Data.Language.Indonesian => IndonesianDictionary(text),
            _ => EnglishDictionary(),
        };
    }

    private static Dictionary<KeyWords, string> EnglishDictionary(string text = "")
    {
        var englishDictionary = Language.EnglishDictionary.Dictionary(text);

        return englishDictionary;
    }

    private static Dictionary<KeyWords, string> FrenchDictionary(string text = "")
    {
        var frenchDictionary = Language.FrenchDictionary.Dictionary(text);

        return frenchDictionary;
    }

    private static Dictionary<KeyWords, string> IndonesianDictionary(string text = "")
    {
        var indonesianDictionary = Language.IndonesianDictionary.Dictionary(text);

        return indonesianDictionary;
    }
}

public enum KeyWords
{
    PasteALink,
    SavedMedias,
    SearchMedias,
    Settings,
    Uploaded,
    Delete,
    Save,
    Videos,
    PlaylistWarning,
    LoadMore,
    Search,
    AppSettings,
    DarkMode,
    DownloadSubtitles,
    AutoSaveVideo,
    CreateSubdirectory,
    RemoveVideoDownload,
    SelectedPath,
    SelectedFormat,
    SelectedResolution,
    SearchLabelText,
    SearchHelperText,
    CancelSearch,
    OpenFolderLocation,
    Clear,
    Video,
    Channel,
    Playlist,
    Download,
    Cancel,
    UpdateData,
    CancelUpdateData,
    DeleteAllWarningTitle,
    DeleteAllWarningText,
    Confirm,
    UrlPlaylistTitle,
    UrlPlaylistText,
    FfmpegErrorMessage,
    Remove,
    OpenDetails,
    OpenUrl,
    Watch,
    SuccessfullySettings,
    FailedSettings,
    FfmpathNotExistError,
    FfmpathCleared,
    SuccessfullyDownloaded,
    SuccessfullySavedData,
    SuccessfullyUpdatedData,
    SuccessfullyDeletedData,
    SuccessfullySavedAllVideos,
    OperationCancelled,
    ErrorWhileSaving,
    ErrorWhileUpdating,
    ErrorWhileLoadingPlaylist,
    ErrorWhileLoadingData,
    EnterPlaylistUrl,
    NoLongerExistsDelete,
    ChannelVideoCount,
    NoVideoErrorMessage,
    DownloadingErrorMessage,
    English,
    French,
    Indonesian,
}