using VidFetchLibrary.Library;

namespace VidFetch.Helpers;
public class VideoLibraryHelper : IVideoLibraryHelper
{
    private readonly IVideoLibrary _videoLibrary;

    public VideoLibraryHelper(IVideoLibrary videoLibrary)
    {
        _videoLibrary = videoLibrary;
    }

    public void ClearVideos(ref double progress)
    {
        _videoLibrary.Videos.Clear();
        progress = 0;
    }

    public void ClearPlaylist(ref double progress)
    {
        _videoLibrary.PlaylistVideos.Clear();
        progress = 0;
    }

    public void ClearAll(ref double videoProgress, ref double playlistProgress)
    {
        ClearVideos(ref videoProgress);
        ClearPlaylist(ref playlistProgress);
    }
}
