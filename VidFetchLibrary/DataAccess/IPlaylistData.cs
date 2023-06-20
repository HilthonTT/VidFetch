using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public interface IPlaylistData
{
    Task<int> DeletePlaylistAsync(PlaylistModel playlist);
    Task<List<PlaylistModel>> GetAllPlaylistsAsync();
    Task<PlaylistModel> GetPlaylistAsync(string url, string playlistId);
    Task<bool> PlaylistExistsAsync(string url, string playlistId);
    Task<int> SetPlaylistAsync(string url, string playlistId);
}