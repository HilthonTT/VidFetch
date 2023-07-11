using VidFetchLibrary.DataAccess;

namespace VidFetchLibrary.Language;
public class LanguageExtension : ILanguageExtension
{
    private readonly ISettingsData _settingsData;
    private readonly IEnglishDictionary _english;
    private readonly IFrenchDictionary _french;
    private readonly IIndonesianDictionary _indonesian;

    public LanguageExtension(ISettingsData settingsData,
                             IEnglishDictionary english,
                             IFrenchDictionary french,
                             IIndonesianDictionary indonesian)
    {
        _settingsData = settingsData;
        _english = english;
        _french = french;
        _indonesian = indonesian;
    }

    public Dictionary<KeyWords, string> GetDictionary(string text = "")
    {
        var settings = _settingsData.GetSettings();

        return settings.SelectedLanguage switch
        {
            Data.Language.English => _english.GetDictionary(text),
            Data.Language.French => _french.GetDictionary(text),
            Data.Language.Indonesian => _indonesian.GetDictionary(text),
            _ => _english.GetDictionary(text),
        };
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
    Channels,
    Playlists,
    PlaylistVideos,
    EnterUrlTextVideo,
    EnterUrlTextChannel,
    EnterUrlTextPlaylist,
    SearchPlaylist,
    SearchVideo,
    SearchChannel,
    SearchPlaylistPlural,
    SearchVideoPlural,
    SearchChannelPlural,
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
    SelectedLanguage,
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
    OperationCanceled,
    ErrorWhileSaving,
    ErrorWhileUpdating,
    ErrorWhileLoadingPlaylist,
    ErrorWhileLoadingData,
    PleaseEnterAPlaylistUrl,
    NoLongerExistsDelete,
    ChannelVideoCount,
    NoVideoErrorMessage,
    DownloadingErrorMessage,
    CurrentlyDownloading,
    DownloadPath,
    VideoFormat,
    Resolution,
    Language,
    FfmpegHelperText,
    Ffmpeg,
    FfmpegSettings,
    SearchTakeLongerWarning,
    Amount,
    English,
    French,
    Indonesian,
}