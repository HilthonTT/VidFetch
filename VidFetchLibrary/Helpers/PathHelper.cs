using VidFetchLibrary.Library;

namespace VidFetchLibrary.Helpers;
public class PathHelper : IPathHelper
{
    private readonly ISettingsLibrary _settings;

    public PathHelper(ISettingsLibrary settings)
    {
        _settings = settings;
    }

    public string GetVideoDownloadPath(string title, bool isPlaylist = false, string playlistTitle = "")
    {
        return _settings.SelectedPath switch
        {
            "Download Folder" => GetDownloadPath(title, _settings.SelectedFormat, isPlaylist, playlistTitle),
            _ => GetSelectedPath(title, _settings.SelectedFormat, _settings.SelectedPath, isPlaylist, playlistTitle),
        };
    }

    public string GetSanizitedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }

    public string OpenFolderLocation() 
    {
        return _settings.SelectedPath switch
        {
            "Download Folder" => GetDownloadPath("", ""),
            _ => GetSelectedPath("", "", _settings.SelectedPath),
        };
    }

    private string GetDownloadPath(
        string title,
        string extension,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        title = GetSanizitedFileName(title);

        string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fileName = title + extension;

        if (isPlaylist && _settings.CreateSubDirectoryPlaylist)
        {
            playlistTitle = GetSanizitedFileName(playlistTitle);
            string videoFolder = OpenFolderLocation();
            string videoFolderPath = Path.Combine(videoFolder, playlistTitle);

            if (Directory.Exists(videoFolderPath) is false)
            {
                Directory.CreateDirectory(videoFolderPath);
            }

            return Path.Combine(videoFolderPath, fileName);
        }

        return Path.Combine(userFolder, "Downloads", fileName);
    }

    private string GetSelectedPath(
        string title,
        string extension,
        string path,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        title = GetSanizitedFileName(title);

        string downloadsFolder = Environment.GetFolderPath(GetFolder(path));
        string fileName = title + extension;

        if (isPlaylist && _settings.CreateSubDirectoryPlaylist)
        {
            playlistTitle = GetSanizitedFileName(playlistTitle);
            string videoFolder = OpenFolderLocation();
            string videoFolderPath = Path.Combine(videoFolder, playlistTitle);

            if (Directory.Exists(videoFolderPath) is false)
            {
                Directory.CreateDirectory(videoFolderPath);
            }

            return Path.Combine(videoFolderPath, fileName);
        }

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
