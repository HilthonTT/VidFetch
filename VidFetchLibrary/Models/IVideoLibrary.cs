using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Models;

public interface IVideoLibrary
{
    List<PlaylistVideo> PlaylistVideos { get; set; }
    List<Video> Videos { get; set; }
}