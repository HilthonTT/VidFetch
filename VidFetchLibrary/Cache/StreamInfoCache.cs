using Microsoft.Extensions.Caching.Memory;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace VidFetchLibrary.Cache;
public class StreamInfoCache : IStreamInfoCache
{
    private const int CacheTime = 5;
    private readonly IMemoryCache _cache;
    private readonly VideoClient _client;

    public StreamInfoCache(IMemoryCache cache, VideoClient client)
    {
        _cache = cache;
        _client = client;
    }

    public async Task<StreamManifest> GetStreamManifestAsync(Video video, CancellationToken token)
    {
        string key = CacheStreamManifest(video.Url);

        var streamManifest = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            return await _client.Streams.GetManifestAsync(video.Id, token);
        });

        if (streamManifest is null)
        {
            _cache.Remove(key);
        }

        return streamManifest;
    }


    public async Task<List<ClosedCaptionTrackInfo>> GetSubtitlesInfoAsync(Video video, CancellationToken token)
    {
        string key = CacheSubtitlesInfo(video.Id);

        var subtitlesInfo = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTime);

            var subtitleManifest = await _client
                .ClosedCaptions
                .GetManifestAsync(video.Id, token);

            var tracks = subtitleManifest.Tracks;

            return tracks.ToList();
        });

        if (subtitlesInfo is null)
        {
            _cache.Remove(key);
        }

        return subtitlesInfo;
    }

    private string CacheStreamManifest(string id)
    {
        return $"{nameof(StreamManifest)}-{id}";
    }

    private string CacheSubtitlesInfo(string id)
    {
        return $"{nameof(ClosedCaptionTrackInfo)}-{id}";
    }
}
