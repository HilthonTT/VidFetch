using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace VidFetchLibrary.Downloader;
public class YoutubeDownloader
{
    public async Task DownloadVideoAsync(string url, string downloadPath)
    {
        var youtube = new YoutubeClient();
        var video = await youtube.Videos.GetAsync(url) ?? throw new Exception("Video not found.");

        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
        var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality() ?? throw new Exception("No suitable video stream found.");

        string sanitizedTitle = GetSanitizedFileName(video.Title);
        string folderPath = GetDownloadFolderPath(sanitizedTitle, downloadPath);
        
        await youtube.Videos.Streams.DownloadAsync(streamInfo, folderPath);
    }

    private static string GetDownloadFolderPath(string sanitizedTitle, string downloadPath)
    {
        string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return downloadPath switch
        {
            "Downloads" => Path.Combine(userFolder, "Downloads", $"{sanitizedTitle}.mp4"),
            "Videos" => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            _ => downloadPath,
        };
    }

    private static string GetSanitizedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }
}
