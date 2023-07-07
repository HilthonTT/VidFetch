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
            Data.Language.French => new(),
            Data.Language.Indonesian => new(),
            _ => EnglishDictionary(),
        };
    }

    private static Dictionary<KeyWords, string> EnglishDictionary(string text = "")
    {
        var englishDictonary = new Dictionary<KeyWords, string>
        {
            { KeyWords.PasteALink, "Paste A Link" },
            { KeyWords.SavedMedias, "Saved Medias" },
            { KeyWords.SearchMedias , "Search Medias" },
            { KeyWords.Settings, "Settings" },
            { KeyWords.Uploaded, "Uploaded" },
            { KeyWords.Delete , "Delete" },
            { KeyWords.Save, "Save" },
            { KeyWords.Videos, "Videos" },
            { KeyWords.PlaylistWarning, "Warning: Only shows 200 videos for performance." },
            { KeyWords.LoadMore, "Load More" },
            { KeyWords.Search, "Search" },
            { KeyWords.AppSettings, "App Settings" },
            { KeyWords.DarkMode, "Dark Mode" },
            { KeyWords.DownloadSubtitles, "Download Subtitles included with the video" },
            { KeyWords.AutoSaveVideo, "Automatically Save Videos" },
            { KeyWords.CreateSubdirectory, "Create Sub Directory for playlist" },
            { KeyWords.RemoveVideoDownload, "Remove Video After Download" },
            { KeyWords.SelectedPath, "Pick your preferred download path." },
            { KeyWords.SelectedFormat, "Pick your preferred format." },
            { KeyWords.SelectedResolution, "Pick your preferred resolution." },
            { KeyWords.SearchLabelText, $"Search your {text}" },
            { KeyWords.SearchHelperText, $"Input the URL or the title of the {text}" },
            { KeyWords.CancelSearch, "Cancel Search" },
            { KeyWords.OpenFolderLocation, "Open Folder Location" },
            { KeyWords.Clear, "Clear" },
            { KeyWords.Video, "Video" },
            { KeyWords.Channel, "Channel" },
            { KeyWords.Playlist, "Playlist" },
            { KeyWords.Download, "Download" },
            { KeyWords.Cancel, "Cancel" },
            { KeyWords.UpdateData, "Update Data" },
            { KeyWords.CancelUpdateData, "Cancel Update Data" },
            { KeyWords.DeleteAllWarningTitle, "Delete All" },
            { KeyWords.DeleteAllWarningText, $"Deleting all of your {text} is irreversible!" },
            { KeyWords.Confirm, "Confirm" },
            { KeyWords.UrlPlaylistTitle, "It appears your URL is a playlist." },
            { KeyWords.UrlPlaylistText, "Would you like to download your video's Url?" },
            { KeyWords.Remove, "Remove" },
            { KeyWords.OpenDetails, "Open Details" },
            { KeyWords.OpenUrl, "Open Url" },
            { KeyWords.Watch, "Watch" },
            { KeyWords.SuccessfullySettings, "Successfully saved settings." },
            { KeyWords.FailedSettings, "Failed to save settings." },
            { KeyWords.FfmpathNotExistError, "Your ffmpeg path doesn't exist." },
            { KeyWords.FfmpegErrorMessage, "Your ffmpeg path is invalid: Your video resolution might be lower." },
            { KeyWords.FfmpathCleared, "Cleared your Ffmpeg path." },
            { KeyWords.SuccessfullySavedData, $"Successfully saved {text}." },
            { KeyWords.SuccessfullyDeletedData, $"Successfully deleted {text}." },
            { KeyWords.SuccessfullyDownloaded, $"Successfully downloaded {text}." },
            { KeyWords.SuccessfullyUpdatedData, $"Successfully updated {text}."},
            { KeyWords.OperationCancelled, "Operation cancelled" },
            { KeyWords.ErrorWhileSaving, "An error occured while saving." },
            { KeyWords.ErrorWhileUpdating, "An error occured while updating." },
            { KeyWords.ErrorWhileLoadingPlaylist, "An error occured while loading your playlist." },
            { KeyWords.ErrorWhileLoadingData, "An occured while loading your data." },
            { KeyWords.NoLongerExistsDelete, $"{text} no longer exists. It has been deleted." },
            { KeyWords.EnterPlaylistUrl, "Please enter a playlist URL" },
            { KeyWords.ChannelVideoCount, $"Only showing {text} videos." },
            { KeyWords.NoVideoErrorMessage, "Error: You have no videos available." },
            { KeyWords.DownloadingErrorMessage, $"There was an issue downloading your videos." },
            { KeyWords.English, "English" },
            { KeyWords.French, "French" },
            { KeyWords.Indonesian, "Indonesian" },
        };

        return englishDictonary;
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