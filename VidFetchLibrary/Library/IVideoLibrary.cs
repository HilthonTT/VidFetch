using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Library;

public interface IVideoLibrary
{
    List<PlaylistVideo> PlaylistVideos { get; set; }
    List<Video> Videos { get; set; }
    List<VideoSearchResult> VideoResults { get; set; }
    List<ChannelSearchResult> ChannelResults { get; set; }
    List<PlaylistSearchResult> PlaylistResults { get; set; }
}