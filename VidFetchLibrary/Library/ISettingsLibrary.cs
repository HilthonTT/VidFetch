namespace VidFetchLibrary.Library;

public interface ISettingsLibrary
{
    bool DownloadSubtitles { get; set; }
    int Id { get; set; }
    bool IsDarkMode { get; set; }
    bool SaveVideos { get; set; }
}