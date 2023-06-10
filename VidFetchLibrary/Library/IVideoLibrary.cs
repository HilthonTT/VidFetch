using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Library;

public interface IVideoLibrary
{
    List<PlaylistVideo> PlaylistVideos { get; set; }
    List<Video> Videos { get; set; }
}