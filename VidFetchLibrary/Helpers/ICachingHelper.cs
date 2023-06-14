namespace VidFetchLibrary.Helpers;

public interface ICachingHelper
{
    string CacheStreamInfoKey(string id);
    string CacheSubtitlesInfoKey(string id);
    string CacheVideoKey(string id);
    string CacheVideoPlaylistKey(string id);
}