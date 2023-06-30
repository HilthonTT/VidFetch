namespace VidFetchLibrary.Helpers;

public interface IPathHelper
{
    string GetSanizitedFileName(string fileName);
    string GetVideoDownloadPath(string title, bool isPlaylist = false, string playlistTitle = "");
    string OpenFolderLocation();
}