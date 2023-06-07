using SQLite;

namespace VidFetchLibrary.Models;
public class SettingsModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public bool IsDarkMode { get; set; }
    public bool DownloadSubtitles { get; set; }
}
