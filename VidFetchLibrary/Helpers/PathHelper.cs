namespace VidFetchLibrary.Helpers;
public class PathHelper : IPathHelper
{
    public string GetVideoDownloadPath(string title, string extension, string path)
    {
        return path switch
        {
            "Download Folder" => GetDownloadPath(title, extension),
            _ => GetSelectedPath(title, extension, path),
        };
    }

    private static string GetDownloadPath(string title, string extension)
    {
        string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fileName = title + extension;
        return Path.Combine(userFolder, "Downloads", fileName);
    }

    private static string GetSelectedPath(string title, string extension, string path)
    {
        string downloadsFolder = Environment.GetFolderPath(GetFolder(path));
        string fileName = title + extension;
        return Path.Combine(downloadsFolder, fileName);
    }

    private static Environment.SpecialFolder GetFolder(string path)
    {
        return path switch
        {
            "Video Folder" => Environment.SpecialFolder.MyVideos,
            "Document Folder" => Environment.SpecialFolder.MyDocuments,
            "Picture Folder" => Environment.SpecialFolder.MyPictures,
            "Music Folder" => Environment.SpecialFolder.MyMusic,
            "Desktop" => Environment.SpecialFolder.Desktop,
            _ => throw new NotImplementedException("No suitable paths."),
        };
    }
}
