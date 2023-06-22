using VidFetchLibrary.Library;
using VidFetchLibrary.Models;

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

    public void RemoveVideo(VideoModel video)
    {
        _videoLibrary.Videos.Remove(video);
    }

    public void RemovePlaylistVideo(VideoModel playlistVideo)
    {
        _videoLibrary.PlaylistVideos.Remove(playlistVideo);
    }

    public void ClearAll(ref double videoProgress, ref double playlistProgress)
    {
        ClearVideos(ref videoProgress);
        ClearPlaylist(ref playlistProgress);
    }
}
