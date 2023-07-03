using VidFetchLibrary.Library;

namespace VidFetchLibrary.Helpers;
public interface IPathHelper
{
    string GetSanizitedFileName(string fileName);
    string GetSpacedString(string value);
    Task<string> GetVideoDownloadPath(SettingsLibrary settings, string title, bool isPlaylist = false, string playlistTitle = "");
    Task<string> OpenFolderLocation();
}