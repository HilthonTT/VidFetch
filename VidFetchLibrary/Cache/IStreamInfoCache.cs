using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace VidFetchLibrary.Cache;
public interface IStreamInfoCache
{
    Task<StreamManifest> GetStreamManifestAsync(Video video, CancellationToken token);
    Task<List<ClosedCaptionTrackInfo>> GetSubtitlesInfoAsync(Video video, CancellationToken token);
}