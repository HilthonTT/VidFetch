using VidFetchLibrary.Models;


namespace VidFetchLibrary.Library;
public class VideoLibrary : IVideoLibrary
{
    public List<VideoModel> Videos { get; set; } = new();
    public List<VideoModel> PlaylistVideos { get; set; } = new();
    public List<ChannelModel> Channels { get; set; } = new();
    public List<PlaylistModel> Playlists { get; set; } = new();
    public List<VideoModel> VideoResults { get; set; } = new();
    public List<ChannelModel> ChannelResults { get; set; } = new();
    public List<PlaylistModel> PlaylistResults { get; set; } = new();
}
