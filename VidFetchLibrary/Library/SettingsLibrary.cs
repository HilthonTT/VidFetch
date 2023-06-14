using SQLite;

namespace VidFetchLibrary.Library;
public class SettingsLibrary : ISettingsLibrary
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public bool IsDarkMode { get; set; }
    public bool DownloadSubtitles { get; set; }
    public bool SaveVideos { get; set; }
}
