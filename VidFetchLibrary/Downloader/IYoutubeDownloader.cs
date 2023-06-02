namespace VidFetchLibrary.Downloader;

public interface IYoutubeDownloader
{
    Task DownloadVideoAsync(string url, string downloadPath);
}