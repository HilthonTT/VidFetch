using YoutubeExplode;
using YoutubeExplode.Videos;
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
        string extension,
        IProgress<double> progress,
        CancellationToken token,
        bool downloadSubtitles = false)
    {
        try
        {
            var video = await client.Videos.GetAsync(videoUrl, token) ?? throw new Exception(VideoNotFoundErrorMessage);

            var streamManifest = await client.Videos.Streams.GetManifestAsync(video.Id, token);
            var streamInfo = streamManifest
                .GetMuxedStreams()
                .GetWithHighestVideoQuality()
                ?? throw new Exception(NoSuitableVideoStreamErrorMessage);

            string sanitizedTitle = GetSanizitedFileName(video.Title);
            string downloadFolder = _pathHelper.GetVideoDownloadPath(sanitizedTitle, extension, path);
            await client.Videos.Streams.DownloadAsync(streamInfo, downloadFolder, progress, token);

            if (downloadSubtitles)
            {
                await DownloadSubtitlesAsync(client, video, path, token);
            }
        }
        catch (TaskCanceledException)
        {
            // Do Nothing as it has been purposely ended.
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private async Task DownloadSubtitlesAsync(
        YoutubeClient client,
        Video video,
        string path, 
        CancellationToken token)
    {
        var subtitleManifest = await client.Videos.ClosedCaptions.GetManifestAsync(video.Id, token);
        var subtitleinfo = subtitleManifest.Tracks;

        if (subtitleinfo is null)
        {
            return;
        }

        foreach (var s in subtitleinfo)
        {
            string sanitizedSubtitle = GetSanizitedFileName($"{s.Language} - {video.Title}");
            string subtitleDownloadFolder = _pathHelper.GetVideoDownloadPath(sanitizedSubtitle, ".vtt", path);
            await client.Videos.ClosedCaptions.DownloadAsync(s, subtitleDownloadFolder, cancellationToken: token);
        }
    }

    private static string GetSanizitedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }
}
