using SQLite;

namespace VidFetchLibrary.Library;
public class SettingsLibrary : ISettingsLibrary
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public bool IsDarkMode { get; set; }
    public bool DownloadSubtitles { get; set; }
    public bool SaveVideos { get; set; }
    public bool CreateSubDirectoryPlaylist { get; set; }
    public string SelectedPath { get; set; }
    public string SelectedFormat { get; set; }
    public string SelectedResolution { get; set; }
    public string FfmpegPath { get; set; }
}
