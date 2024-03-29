﻿using Microsoft.Extensions.Caching.Memory;

namespace VidFetchLibrary.Language;
public class EnglishDictionary : IEnglishDictionary
{
    private const string CacheName = nameof(EnglishDictionary);
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;

    public EnglishDictionary(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Dictionary<KeyWords, string> GetDictionary(string text)
    {
        string key = $"{CacheName}-{text}";

        var dictionary = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            return CreateDictionary(text);
        });

        if (dictionary is null)
        {
            _cache.Remove(key);
        }

        return dictionary;
    }

    private static Dictionary<KeyWords, string> CreateDictionary(string text)
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
            { KeyWords.Channels, "Channels" },
            { KeyWords.Playlists, "Playlists" },
            { KeyWords.PlaylistVideos, "Playlist Videos" },
            { KeyWords.EnterUrlTextVideo, "Enter the video's URL" },
            { KeyWords.EnterUrlTextChannel, "Enter the channel's URL" },
            { KeyWords.EnterUrlTextPlaylist, "Enter the playlist's URL" },
            { KeyWords.SearchVideo, $"Search video" },
            { KeyWords.SearchChannel, $"Search channel" },
            { KeyWords.SearchPlaylist, $"Search playlist" },
            { KeyWords.SearchVideoPlural, $"Search {text} videos" },
            { KeyWords.SearchChannelPlural, $"Search {text} channels" },
            { KeyWords.SearchPlaylistPlural, $"Search {text} playlists" },
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
            { KeyWords.SelectedLanguage, "Pick your preferred language." },
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
            { KeyWords.SuccessfullySavedAllVideos, $"Successfully saved all videos" },
            { KeyWords.OperationCanceled, "Operation canceled." },
            { KeyWords.ErrorWhileSaving, "An error occured while saving." },
            { KeyWords.ErrorWhileUpdating, "An error occured while updating." },
            { KeyWords.ErrorWhileLoadingPlaylist, "An error occured while loading your playlist." },
            { KeyWords.ErrorWhileLoadingData, "An occured while loading your data." },
            { KeyWords.NoLongerExistsDelete, $"{text} no longer exists. It has been deleted." },
            { KeyWords.PleaseEnterAPlaylistUrl, "Please enter a playlist URL" },
            { KeyWords.ChannelVideoCount, $"Only showing {text} videos." },
            { KeyWords.NoVideoErrorMessage, "Error: You have no videos available." },
            { KeyWords.DownloadingErrorMessage, $"There was an issue downloading your videos." },
            { KeyWords.CurrentlyDownloading, $"Currently downloading: {text}" },
            { KeyWords.DownloadPath, "Download Path:" },
            { KeyWords.VideoFormat, "Video Format:" },
            { KeyWords.Resolution, "Resolution:" },
            { KeyWords.Language, "Language:" },
            { KeyWords.FfmpegHelperText, "Input your ffmpeg file path to the .exe" },
            { KeyWords.Ffmpeg, "Ffmpeg File Path" },
            { KeyWords.FfmpegSettings, "Ffmpeg Settings" },
            { KeyWords.SearchTakeLongerWarning, "Searching might take longer depending on the amount." },
            { KeyWords.Amount, "Amount" },
            { KeyWords.SearchLoadedItems, "Search your loaded items." },
            { KeyWords.English, "English" },
            { KeyWords.French, "French" },
            { KeyWords.Indonesian, "Indonesian" },
        };

        return englishDictonary;
    }
}
