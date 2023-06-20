namespace VidFetchLibrary.Helpers;

public interface ICachingHelper
{
    string CacheChannelKey(string id);
    string CachePlaylistKey(string id);
    string CacheStreamInfoKey(string id);
    string CacheSubtitlesInfoKey(string id);
    string CacheVideoKey(string id);
    string CacheVideoPlaylistKey(string id);
}