using VidFetchLibrary.Models;

namespace VidFetchLibrary.Cache;
public interface IPlaylistCache
{
    Task<PlaylistModel> GetPlaylistAsync(string url, CancellationToken token = default);
    Task<List<PlaylistModel>> GetPlaylistsBySearchAsync(string searchInput, int count, CancellationToken token = default);
}