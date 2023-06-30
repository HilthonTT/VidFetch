using SQLite;
using VidFetchLibrary.Data;

namespace VidFetchLibrary.Library;
public class SettingsLibrary : ISettingsLibrary
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public bool IsDarkMode { get; set; }
    public bool DownloadSubtitles { get; set; }
    public bool SaveVideos { get; set; }
    public bool CreateSubDirectoryPlaylist { get; set; }
    public DownloadPath SelectedPath { get; set; }
    public VideoExtension SelectedFormat { get; set; }
    public VideoResolution SelectedResolution { get; set; }
    public string FfmpegPath { get; set; }
}
