using VidFetchLibrary.Models;

namespace VidFetchLibrary.Library;

public interface IVideoLibrary
{
    List<VideoModel> PlaylistVideos { get; set; }
    List<VideoModel> Videos { get; set; }
    List<VideoModel> VideoResults { get; set; }
    List<ChannelModel> ChannelResults { get; set; }
    List<PlaylistModel> PlaylistResults { get; set; }
    List<ChannelModel> Channels { get; set; }
    List<PlaylistModel> Playlists { get; set; }
}