using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos.Streams;

namespace VidFetchLibrary.Helpers;
public class DownloadHelper : IDownloadHelper
{
    private const string VideoNotFoundErrorMessage = "Video not found.";
    private const string NoVideosFoundErrorMessage = "No videos found in the playlist";
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

    public async Task DownloadSelectedVideoAsync(
        YoutubeClient client,
        string path,
        string extension,
        PlaylistVideo playlistVideo)
    {
        var streamManifest = await client.Videos.Streams.GetManifestAsync(playlistVideo.Id);
        var streamInfo = streamManifest
            .GetMuxedStreams()
            .GetWithHighestVideoQuality()
            ?? throw new Exception(NoSuitableVideoStreamErrorMessage);

        string sanitizedTitle = GetSanizitedFileName(playlistVideo.Title);
        string downloadFolder = _pathHelper.GetVideoDownloadPath(sanitizedTitle, extension, path);
        await client.Videos.Streams.DownloadAsync(streamInfo, downloadFolder);
    }

    public async Task DownloadVideoFromPlaylistAsync(
        YoutubeClient client,
        List<PlaylistVideo> playlistVideos,
        int videoIndex,
        string path,
        string extension)
    {
        if (playlistVideos.Any() is false)
        {
            throw new Exception(NoVideosFoundErrorMessage);
        }

        if (videoIndex < 1 || videoIndex > playlistVideos.Count)
        {
            throw new Exception("Invalid video index");
        }

        var video = playlistVideos[videoIndex - 1];
        await DownloadVideoAsync(client, video.Url, path, extension);
    }

    public async Task DownloadPlaylistAsync(
        YoutubeClient client,
        List<PlaylistVideo> playlistVideos,
        string path,
        string extension,
        CancellationToken cancellationToken)
    {
        if (playlistVideos.Any() is false)
        {
            throw new Exception(NoVideosFoundErrorMessage);
        }

        foreach (var v in playlistVideos)
        {
            if (cancellationToken.IsCancellationRequested) 
            {
                break;
            }

            await DownloadVideoAsync(client, v.Url, path, extension);
        }
    }

    private static string GetSanizitedFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalidChars.Contains(c) ? '_' : c));
    }
}
