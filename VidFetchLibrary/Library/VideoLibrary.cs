using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Library;
public class VideoLibrary : IVideoLibrary
{
    public List<Video> Videos { get; set; } = new();
    public List<PlaylistVideo> PlaylistVideos { get; set; } = new();
}
