using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Library;

namespace VidFetchLibrary.Language;
public class LanguageExtension : ILanguageExtension
{
    private readonly ISettingsData _settingsData;

    public LanguageExtension(ISettingsData settingsData)
    {
        _settingsData = settingsData;
    }

    public Dictionary<KeyWords, string> GetDictionary()
    {
        var settings = new SettingsLibrary();
        // temporary solution

        return settings.SelectedLanguage switch
        {
            Data.Language.English => EnglishDictionary(),
            Data.Language.French => new(),
            Data.Language.Indonesian => new(),
            _ => EnglishDictionary(),
        };
    }

    private static Dictionary<KeyWords, string> EnglishDictionary()
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
            { KeyWords.DownloadSubtitles, "Download Subtitles included with video" },
            { KeyWords.AutoSaveVideo, "Automatically Save Videos" },
            { KeyWords.CreateSubdirectory, "Create Sub Directory for playlist" },
            { KeyWords.RemoveVideoDownload, "Remove Video After Download" },
            { KeyWords.SelectedPath, "Pick your preferred download path." },
            { KeyWords.SelectedFormat, "Pick your preferred format." },
            { KeyWords.SelectedResolution, "Pick your preferred resolution." },
            { KeyWords.SearchLabelText, "Search by inputing the title of the " },
            { KeyWords.SearchHelperText, "Input using the URL or title of the " },
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
            { KeyWords.DeleteAllWarningText, "Deleting all of your data is irreversible!" },
            { KeyWords.Confirm, "Confirm" },
            { KeyWords.UrlPlaylistTitle, "It appears your URL is a playlist." },
            { KeyWords.UrlPlaylistText, "Would you like to download your video's Url?" },
            { KeyWords.FfmpegErrorMessage, "Your ffmpeg path is invalid: Your video resolution might be lower." },
            { KeyWords.Remove, "Remove" },
            { KeyWords.OpenDetails, "Open Details" },
            { KeyWords.OpenUrl, "Open Url" },
            { KeyWords.Watch, "Watch" },
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
    English,
    French,
    Indonesian,
}