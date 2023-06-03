namespace VidFetchLibrary.Helpers;
public class PathHelper : IPathHelper
{
    public string GetVideoDownloadPath(string title, string extension, string path)
    {
        return path switch
        {
            "Download Folder" => GetDownloadPath(title, extension),
            "Custom" => GetCustomPath(title, extension, path),
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

    private static string GetCustomPath(string title, string extension, string path)
    {
        string fileName = title + extension;
        return Path.Combine(path, fileName);
    }

    private static Environment.SpecialFolder GetFolder(string path)
    {
        return path switch
        {
            "Video Folder" => Environment.SpecialFolder.MyVideos,
            "Document Folder" => Environment.SpecialFolder.MyDocuments,
            "Picture Folder" => Environment.SpecialFolder.MyPictures,
            "Desktop" => Environment.SpecialFolder.Desktop,
            _ => throw new NotImplementedException("No suitable paths."),
        };
    }
}
