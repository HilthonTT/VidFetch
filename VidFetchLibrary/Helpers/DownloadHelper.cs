using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace VidFetchLibrary.Helpers;
public class DownloadHelper : IDownloadHelper
{
    private const string VideoNotFoundErrorMessage = "Video not found.";
    private const string NoSuitableVideoStreamErrorMessage = "No suitable video stream found";
    private readonly IPathHelper _pathHelper;

    public DownloadHelper(IPathHelper pathHelper)
    {
        _pathHelper = pathHelper;
    }

    public async Task DownloadVideoAsync(
        YoutubeClient client,
        string videoUrl,
        string path,
        string extension)
    {
        var video = await client.Videos.GetAsync(videoUrl) ?? throw new Exception(VideoNotFoundErrorMessage);

        var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id);
        var streamInfo = streamManifest
            .GetMuxedStreams()
            .GetWithHighestVideoQuality() 
            ?? throw new Exception(NoSuitableVideoStreamErrorMessage);

        string sanitizedTitle = GetSanizitedFileName(video.Title);
        string downloadFolder = _pathHelper.GetVideoDownloadPath(sanitizedTitle, extension, path);
        await client.Videos.Streams.DownloadAsync(streamInfo, downloadFolder);
    }

    private static string GetSanizitedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }
}
