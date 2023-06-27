namespace VidFetchLibrary.Helpers;

public interface IPathHelper
{
    string GetVideoDownloadPath(string title);
    string OpenFolderLocation();
}