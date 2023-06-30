using System.Text.RegularExpressions;
using VidFetchLibrary.Data;
using VidFetchLibrary.Library;

namespace VidFetchLibrary.Helpers;
public partial class PathHelper : IPathHelper
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
            DownloadPath.DownloadFolder => GetDownloadPath(title, isPlaylist, playlistTitle),
            _ => GetSelectedPath(title, isPlaylist, playlistTitle),
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
            DownloadPath.DownloadFolder => GetDownloadPath(""),
            _ => GetSelectedPath(""),
        };
    }

    public string GetSpacedString(string value)
    {
        string selectedPathString = PathRegex().Replace(value.ToString(), " $1");

        return selectedPathString;
    }

    private string GetDownloadPath(
        string title,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        title = GetSanizitedFileName(title);

        string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fileName = GetFileNameAndExtension(title);

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
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        title = GetSanizitedFileName(title);

        string downloadsFolder = Environment.GetFolderPath(GetFolder());
        string fileName = GetFileNameAndExtension(title);

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

    private Environment.SpecialFolder GetFolder()
    {

        return _settings.SelectedPath switch
        {
            DownloadPath.VideoFolder => Environment.SpecialFolder.MyVideos,
            DownloadPath.DocumentFolder => Environment.SpecialFolder.MyDocuments,
            DownloadPath.PictureFolder => Environment.SpecialFolder.MyPictures,
            DownloadPath.MusicFolder => Environment.SpecialFolder.MyMusic,
            DownloadPath.Desktop => Environment.SpecialFolder.Desktop,
            _ => throw new NotImplementedException("No suitable paths."),
        };
    }

    private string GetFileNameAndExtension(string title)
    {
        string fileExtension = _settings.SelectedFormat
            .ToString()
            .ToLower()
            .TrimStart('_');

        return $"{title}.{fileExtension}";
    }

    [GeneratedRegex("(\\B[A-Z])")]
    private static partial Regex PathRegex();
}
