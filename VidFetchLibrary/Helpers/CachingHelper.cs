namespace VidFetchLibrary.Helpers;
public class CachingHelper : ICachingHelper
{
    public string CacheVideoList(string url)
    {
        return $"VideoListData-{url}";
    }

    public string CacheMainVideoKey(string url)
    {
        return $"MainVideoData-{url}";
    }

    public string CacheVideoKey(string url)
    {
        return $"VideoData-{url}";
    }

    public string CacheChannelKey(string url)
    {
        return $"ChannelData-{url}";
    }

    public string CachePlaylistKey(string url)
    {
        return $"PlaylistData-{url}";
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
