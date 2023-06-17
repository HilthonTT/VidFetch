namespace VidFetchLibrary.Helpers;
public class CachingHelper : ICachingHelper
{
    private const string CacheNameVideo = "VideoData";
    private const string CacheNamePlaylistVideo = "PlaylistVideoData";
    private const string CacheNameChannel = "ChannelData";
    private const string CacheNameStreamInfo = "StreamInfoData";
    private const string CacheNameSubtitlesInfo = "SubtitlesInfoData";

    public string CacheVideoKey(string id)
    {
        return $"{CacheNameVideo}-{id}";
    }

    public string CacheVideoPlaylistKey(string id)
    {
        return $"{CacheNamePlaylistVideo}-{id}";
    }

    public string CacheChannelKey(string id)
    {
        return $"{CacheNameChannel}-{id}";
    }

    public string CacheStreamInfoKey(string id)
    {
        return $"{CacheNameStreamInfo}-{id}";
    }

    public string CacheSubtitlesInfoKey(string id)
    {
        return $"{CacheNameSubtitlesInfo}-{id}";
    }
}
