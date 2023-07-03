using System.Text.RegularExpressions;
using VidFetchLibrary.Data;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Library;

namespace VidFetchLibrary.Helpers;
public partial class PathHelper : IPathHelper
{
    private readonly ISettingsData _settingsData;

    public PathHelper(ISettingsData settingsData)
    {
        _settingsData = settingsData;
    }

    public async Task<string> GetVideoDownloadPath(
        SettingsLibrary settings,
        string title,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        return settings.SelectedPath switch
        {
            DownloadPath.DownloadFolder => await GetDownloadPath(settings, title, isPlaylist, playlistTitle),
            _ => await GetSelectedPath(settings, title, isPlaylist, playlistTitle),
        };
    }

    public string GetSanizitedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }

    public async Task<string> OpenFolderLocation()
    {
        var settings = await _settingsData.GetSettingsAsync();

        string path = settings.SelectedPath switch
        {
            DownloadPath.DownloadFolder => await GetDownloadPath(settings, ""),
            _ => await GetSelectedPath(settings, ""),
        };

        int dotIndex = path.LastIndexOf('.');

        if (dotIndex >= 0)
        {
            string trimmedFilePath = path[..dotIndex];
            path = trimmedFilePath;
        }

        return path;
    }

    public string GetSpacedString(string value)
    {
        string selectedPathString = PathRegex().Replace(value.ToString(), " $1");

        return selectedPathString;
    }

    private async Task<string> GetDownloadPath(
        SettingsLibrary settings,
        string title,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        title = GetSanizitedFileName(title);

        string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fileName = GetFileNameAndExtension(title, settings);

        if (isPlaylist && settings.CreateSubDirectoryPlaylist)
        {
            playlistTitle = GetSanizitedFileName(playlistTitle);
            string videoFolder = await OpenFolderLocation();
            string videoFolderPath = Path.Combine(videoFolder, playlistTitle);

            if (Directory.Exists(videoFolderPath) is false)
            {
                Directory.CreateDirectory(videoFolderPath);
            }

            return Path.Combine(videoFolderPath, fileName);
        }

        return Path.Combine(userFolder, "Downloads", fileName);
    }

    private async Task<string> GetSelectedPath(
        SettingsLibrary settings,
        string title,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        title = GetSanizitedFileName(title);

        string downloadsFolder = Environment.GetFolderPath(GetFolder(settings));
        string fileName = GetFileNameAndExtension(title, settings);

        if (isPlaylist && settings.CreateSubDirectoryPlaylist)
        {
            playlistTitle = GetSanizitedFileName(playlistTitle);
            string videoFolder = await OpenFolderLocation();
            string videoFolderPath = Path.Combine(videoFolder, playlistTitle);

            if (Directory.Exists(videoFolderPath) is false)
            {
                Directory.CreateDirectory(videoFolderPath);
            }

            return Path.Combine(videoFolderPath, fileName);
        }

        return Path.Combine(downloadsFolder, fileName);
    }

    private Environment.SpecialFolder GetFolder(SettingsLibrary settings)
    {
        return settings.SelectedPath switch
        {
            DownloadPath.VideoFolder => Environment.SpecialFolder.MyVideos,
            DownloadPath.DocumentFolder => Environment.SpecialFolder.MyDocuments,
            DownloadPath.PictureFolder => Environment.SpecialFolder.MyPictures,
            DownloadPath.MusicFolder => Environment.SpecialFolder.MyMusic,
            DownloadPath.Desktop => Environment.SpecialFolder.Desktop,
            _ => throw new NotImplementedException("No suitable paths."),
        };
    }

    private string GetFileNameAndExtension(string title, SettingsLibrary settings)
    {
        string fileExtension = settings.SelectedFormat
            .ToString()
            .ToLower()
            .TrimStart('_');

        return $"{title}.{fileExtension}";
    }

    [GeneratedRegex("(\\B[A-Z])")]
    private static partial Regex PathRegex();
}
