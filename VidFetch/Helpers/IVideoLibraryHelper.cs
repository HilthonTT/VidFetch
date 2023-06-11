namespace VidFetch.Helpers;

public interface IVideoLibraryHelper
{
    void ClearAll(ref double videoProgress, ref double playlistProgress);
    void ClearPlaylist(ref double progress);
    void ClearVideos(ref double progress);
}