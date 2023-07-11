using VidFetchLibrary.Library;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using VidFetchLibrary.Data;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Cache;
using YoutubeExplode;

namespace VidFetchLibrary.Helpers;
public class DownloadHelper : IDownloadHelper
{
    private readonly IPathHelper _pathHelper;
    private readonly ISettingsData _settingsData;
    private readonly IStreamInfoCache _streamInfoCache;
    private readonly YoutubeClient _youtube;

    public DownloadHelper(
        IPathHelper pathHelper,
        ISettingsData settingsData,
        IStreamInfoCache streamInfoCache,
        YoutubeClient youtube)
    {
        _pathHelper = pathHelper;
        _settingsData = settingsData;
        _streamInfoCache = streamInfoCache;
        _youtube = youtube;
    }

    public async Task DownloadVideoAsync(
        string videoUrl,
        IProgress<double> progress,
        CancellationToken token,
        bool isPlaylist = false,
        string playlistTitle = "")
    {
        try
        {
            var video = await _youtube.Videos.GetAsync(videoUrl, token)
                ?? throw new NullReferenceException("Video not found.");

            var settings = await _settingsData.GetSettingsAsync();

            string path = settings.FfmpegPath;
            if (File.Exists(path) && string.IsNullOrWhiteSpace(path) is false)
            {
                await DownloadWithFfmpeg(
                    video,
                    settings,
                    isPlaylist,
                    playlistTitle,
                    progress,
                    token);
            }
            else
            {
                await DownloadWithoutFfmpeg(
                    video,
                    settings,
                    videoUrl,
                    isPlaylist,
                    playlistTitle,
                    progress,
                    token);
            }

            if (settings.DownloadSubtitles)
            {
                await DownloadSubtitlesAsync(video, token);
            }
        }
        catch (TaskCanceledException)
        {
            throw new OperationCanceledException();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private async Task DownloadWithFfmpeg(
        Video video,
        SettingsLibrary settings,
        bool isPlaylist,
        string playlistTitle,
        IProgress<double> progress, 
        CancellationToken token)
    {
        var streamInfos = await LoadStreamInfosAsync(video, settings, token);
        var conversionRequest = await GetRequestBuilder(settings, video, isPlaylist, playlistTitle);

        await _youtube.Videos.DownloadAsync(streamInfos, conversionRequest, progress, token);
    }

    private async Task DownloadWithoutFfmpeg(
        Video video,
        SettingsLibrary settings,
        string videoUrl,
        bool isPlaylist,
        string playlistTitle,
        IProgress<double> progress,
        CancellationToken token)
    {
        var streamManifest = await _youtube.Videos.Streams
            .GetManifestAsync(videoUrl, token);

        var streamInfo = GetVideoStream(streamManifest, settings);

        string downloadFolder = await _pathHelper.GetVideoDownloadPath(
            settings,
            video.Title,
            isPlaylist,
            playlistTitle);

        await _youtube.Videos.Streams.DownloadAsync(streamInfo, downloadFolder, progress, token);
    }

    private async Task DownloadSubtitlesAsync(
        Video video,
        CancellationToken token)
    {
        var subtitleinfo = await _streamInfoCache.GetSubtitlesInfoAsync(video, token);

        if (subtitleinfo is null || subtitleinfo.Count <= 0)
        {
            return;
        }

        string videoFolder = await _pathHelper.OpenFolderLocation();
        string sanitizedVideoTitle = _pathHelper.GetSanizitedFileName(video.Title);
        string videoFolderPath = Path.Combine(videoFolder, sanitizedVideoTitle);

        if (Directory.Exists(videoFolderPath) is false)
        {
            Directory.CreateDirectory(videoFolderPath);
        }

        foreach (var s in subtitleinfo)
        {
            string sanitizedSubtitle = _pathHelper.GetSanizitedFileName($"{s.Language}");
            string subtitleDownloadFolder = Path.Combine(videoFolderPath, $"{sanitizedSubtitle}.vtt");

            await _youtube.Videos.ClosedCaptions.DownloadAsync(s, subtitleDownloadFolder, cancellationToken: token);
        }
    }

    private async Task<IStreamInfo[]> LoadStreamInfosAsync(
        Video video,
        SettingsLibrary settings,
        CancellationToken token)
    {
        var streamManifest = await _streamInfoCache.GetStreamManifestAsync(video, token);

        var audioStreamInfo = streamManifest
            .GetAudioStreams()
            .Where(s => s.Container == Container.Mp4)
            .GetWithHighestBitrate();

        var videoStreamInfo = GetFfmpegVideoStream(streamManifest, settings);

        return new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
    }


    private async Task<ConversionRequest> GetRequestBuilder(
        SettingsLibrary settings,
        Video video,
        bool isPlaylist,
        string playlistTitle)
    {
        string downloadFolder = await _pathHelper.GetVideoDownloadPath(settings, video.Title, isPlaylist, playlistTitle);

        var requestBuilder = new ConversionRequestBuilder(downloadFolder)
            .SetFFmpegPath(settings.FfmpegPath)
            .SetPreset(ConversionPreset.UltraFast)
            .SetContainer(Container.Mp4)
            .Build();

        return requestBuilder;
    }

    private static IVideoStreamInfo GetFfmpegVideoStream(
        StreamManifest streamManifest,
        SettingsLibrary settings)
    {
        var highestVideoResolutionStream = streamManifest
            .GetVideoStreams()
            .GetWithHighestVideoQuality();
        try
        {
            var videoStreamInfo = settings.SelectedResolution switch
            {
                VideoResolution.HighestResolution => highestVideoResolutionStream,

                _ => streamManifest.GetVideoStreams()
                    .First(s => s.VideoQuality.Label == GetVideoQualityLabel(settings)),
            };

            return videoStreamInfo;
        }
        catch
        {
            return highestVideoResolutionStream;
        }
    }

    private static IVideoStreamInfo GetVideoStream(
        StreamManifest streamManifest,
        SettingsLibrary settings)
    {
        var highestResolutionStream = streamManifest.GetMuxedStreams()
                .GetWithHighestVideoQuality();
        try
        {
            var videoStreamInfo = settings.SelectedResolution switch
            {
                VideoResolution.HighestResolution => highestResolutionStream,

                _ => streamManifest.GetMuxedStreams()
                    .First(s => s.VideoQuality.Label == GetVideoQualityLabel(settings)),
            };

            return videoStreamInfo;
        }
        catch
        {
            return highestResolutionStream;
        }
    }

    private static string GetVideoQualityLabel(SettingsLibrary settings)
    {
        var resolution = settings.SelectedResolution;
        string resolutionString = resolution.ToString().ToLower();

        return resolutionString switch
        {
            "p144" => "144p",
            "p240" => "240p",
            "p360" => "360p",
            "p480" => "480p",
            "p720" => "720p",
            "p1080" => "1080p",
            "p2160" => "2160p",
            _ => "HighestResolution",
        };
    }
}
