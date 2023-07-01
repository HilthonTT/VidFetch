namespace VidFetchLibrary.Helpers;

public interface IDownloadHelper
{
    Task DownloadVideoAsync(string videoUrl, IProgress<double> progress, CancellationToken token, bool isPlaylist = false, string playlistTitle = "");
}