using SQLite;
using VidFetchLibrary.Data;

namespace VidFetchLibrary.Library;
public class SettingsLibrary : ISettingsLibrary
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public bool IsDarkMode { get; set; } = true;
    public bool DownloadSubtitles { get; set; } = false;
    public bool SaveVideos { get; set; } = false;
    public bool CreateSubDirectoryPlaylist { get; set; } = true;
    public bool RemoveAfterDownload { get; set; } = false;
    public DownloadPath SelectedPath { get; set; }
    public VideoExtension SelectedFormat { get; set; }
    public VideoResolution SelectedResolution { get; set; }
    public string FfmpegPath { get; set; } = "";


    public SettingsLibrary()
    {

    }

    public SettingsLibrary(SettingsLibrary settings)
    {
        Id = settings.Id;
        IsDarkMode = settings.IsDarkMode;
        DownloadSubtitles = settings.DownloadSubtitles;
        SaveVideos = settings.SaveVideos;
        CreateSubDirectoryPlaylist = settings.CreateSubDirectoryPlaylist;
        RemoveAfterDownload = settings.RemoveAfterDownload;
        SelectedPath = settings.SelectedPath;
        SelectedFormat = settings.SelectedFormat;
        SelectedResolution = settings.SelectedResolution;
        FfmpegPath = settings.FfmpegPath;
    }
}
