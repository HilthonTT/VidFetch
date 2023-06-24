using VidFetchLibrary.Models;

namespace VidFetch.Helpers;

public interface IVideoLibraryHelper
{
    void ClearAll(ref double videoProgress, ref double playlistProgress);
    void ClearChannels();
    void ClearPlaylistVideos(ref double progress);
    void ClearPlaylists();
    void ClearVideos(ref double progress);
    void RemoveChannel(ChannelModel channel);
    void RemovePlaylistVideo(VideoModel playlistVideo);
    void RemoveVideo(VideoModel video);
    void RemovePlaylist(PlaylistModel playlist);
}