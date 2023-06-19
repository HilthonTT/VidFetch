using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Library;
public class VideoLibrary : IVideoLibrary
{
    public List<Video> Videos { get; set; } = new();
    public List<PlaylistVideo> PlaylistVideos { get; set; } = new();
    public List<VideoSearchResult> VideoResults { get; set; } = new();
    public List<ChannelSearchResult> ChannelResults { get; set; } = new();
    public List<PlaylistSearchResult> PlaylistResults { get; set; } = new();
}
