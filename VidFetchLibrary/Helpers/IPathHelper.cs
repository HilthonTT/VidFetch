namespace VidFetchLibrary.Helpers;

public interface IPathHelper
{
    string GetFfmpegPath();
    string GetVideoDownloadPath(string title);
}