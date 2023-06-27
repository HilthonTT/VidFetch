using System.Reflection;
using VidFetchLibrary.Library;

namespace VidFetchLibrary.Helpers;
public class PathHelper : IPathHelper
{
    private readonly ISettingsLibrary _settings;

    public PathHelper(ISettingsLibrary settings)
    {
        _settings = settings;
    }

    public string GetVideoDownloadPath(string title)
    {
        return _settings.SelectedPath switch
        {
            "Download Folder" => GetDownloadPath(title, _settings.SelectedFormat),
            _ => GetSelectedPath(title, _settings.SelectedFormat, _settings.SelectedPath),
        };
    }

    public string OpenFolderLocation() 
    {
        return _settings.SelectedPath switch
        {
            "Download Folder" => GetDownloadPath("", ""),
            _ => GetSelectedPath("", "", _settings.SelectedPath),
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
