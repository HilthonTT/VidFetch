using YoutubeExplode;

namespace VidFetchLibrary.Helpers;
public interface IDownloadHelper
{
    Task DownloadVideoAsync(YoutubeClient client, string videoUrl, string path, string extension);
}