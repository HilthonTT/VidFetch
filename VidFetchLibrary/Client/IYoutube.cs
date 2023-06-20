using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Client;

public interface IYoutube
{
    Task DownloadVideoAsync(string url, string downloadPath, string extension, IProgress<double> progress, CancellationToken token, bool downloadSubtitles = false);
    Task<Channel> GetChannelAsync(string url);
    Task<List<ChannelSearchResult>> GetChannelBySearchAsync(string searchInput, CancellationToken token);
    Task<Playlist> GetPlaylistAsync(string url);
    Task<List<PlaylistSearchResult>> GetPlaylistsBySearchAsync(string searchInput, CancellationToken token);
    Task<List<PlaylistVideo>> GetPlayListVideosAsync(string url);
    Task<Video> GetVideoAsync(string url);
    Task<List<VideoSearchResult>> GetVideosBySearchAsync(string searchInput, CancellationToken token);
}