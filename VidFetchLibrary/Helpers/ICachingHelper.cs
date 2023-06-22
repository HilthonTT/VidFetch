namespace VidFetchLibrary.Helpers;
public interface ICachingHelper
{
    string CacheChannelKey(string url);
    string CacheMainVideoKey(string url);
    string CachePlaylistKey(string url);
    string CacheStreamInfoKey(string id);
    string CacheSubtitlesInfoKey(string id);
    string CacheVideoKey(string url);
    string CacheVideoList(string url);
}