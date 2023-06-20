namespace VidFetchLibrary.Helpers;
public class CachingHelper : ICachingHelper
{
    public string CacheVideoKey(string id)
    {
        return $"VideoData-{id}";
    }

    public string CacheVideoPlaylistKey(string id)
    {
        return $"PlaylistVideoData-{id}";
    }

    public string CacheChannelKey(string id)
    {
        return $"ChannelData-{id}";
    }

    public string CachePlaylistKey(string id)
    {
        return $"PlaylistData-{id}";
    }

    public string CacheStreamInfoKey(string id)
    {
        return $"StreamInfoData-{id}";
    }

    public string CacheSubtitlesInfoKey(string id)
    {
        return $"SubtitlesInfoData-{id}";
    }
}
