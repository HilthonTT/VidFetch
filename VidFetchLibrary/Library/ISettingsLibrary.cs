using VidFetchLibrary.Data;

namespace VidFetchLibrary.Library;

public interface ISettingsLibrary
{
    bool DownloadSubtitles { get; set; }
    int Id { get; set; }
    bool IsDarkMode { get; set; }
    bool SaveVideos { get; set; }
    DownloadPath SelectedPath { get; set; }
    VideoExtension SelectedFormat { get; set; }
    VideoResolution SelectedResolution { get; set; }
    string FfmpegPath { get; set; }
    bool CreateSubDirectoryPlaylist { get; set; }
    bool RemoveAfterDownload { get; set; }
}