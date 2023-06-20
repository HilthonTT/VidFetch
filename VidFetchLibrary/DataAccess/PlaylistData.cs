using Microsoft.Extensions.Caching.Memory;
using SQLite;
using VidFetchLibrary.Client;
using VidFetchLibrary.Models;

namespace VidFetchLibrary.DataAccess;
public class PlaylistData : IPlaylistData
{
    private const string DbName = "Playlist.db3";
    private const string CacheName = "PlaylistData";
    private readonly IMemoryCache _cache;
    private readonly IYoutube _youtube;
    private SQLiteAsyncConnection _db;

    public PlaylistData(IMemoryCache cache,
                        IYoutube youtube)
    {
        _cache = cache;
        _youtube = youtube;
        SetUpDb();
    }

    private void SetUpDb()
    {
        if (_db is null)
        {
            string dbPath = Path
                .Combine(Environment
                .GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData), DbName);

            _db = new(dbPath);
            _db.CreateTableAsync<PlaylistModel>();
        }
    }

    public async Task<List<PlaylistModel>> GetAllPlaylistsAsync()
    {
        var output = _cache.Get<List<PlaylistModel>>(CacheName);
        if (output is null)
        {
            output = await _db.Table<PlaylistModel>().ToListAsync();
            _cache.Set(CacheName, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<PlaylistModel> GetPlaylistAsync(string url, string playlistId)
    {
        string key = GetCache(playlistId);

        var output = _cache.Get<PlaylistModel>(key);
        if (output is null)
        {
            output = await _db.Table<PlaylistModel>().FirstOrDefaultAsync(v => v.PlaylistId == playlistId || v.Url == url);
            _cache.Set(key, output, TimeSpan.FromHours(5));
        }

        return output;
    }

    public async Task<bool> PlaylistExistsAsync(string url, string playlistId)
    {
        var playlist = await GetPlaylistAsync(url, playlistId);
        if (playlist is null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task<int> SetPlaylistAsync(string url, string playlistId)
    {
        var existingPlaylist = await GetPlaylistAsync(url, playlistId);
        RemoveCache();

        if (existingPlaylist is null)
        {
            var playlist = await _youtube.GetPlaylistAsync(url);
            return await CreatePlaylistAsync(new PlaylistModel(playlist));
        }
        else
        {
            return await UpdatePlaylistAsync(existingPlaylist);
        }
    }

    public async Task<int> DeletePlaylistAsync(PlaylistModel playlist)
    {
        string key = GetCache(playlist.PlaylistId);

        var existingPlaylist = await GetPlaylistAsync(playlist.Url, playlist.PlaylistId);

        if (existingPlaylist is not null)
        {
            RemoveCache(key);
            return await _db.DeleteAsync<VideoModel>(existingPlaylist.Id);
        }
        else
        {
            return 0;
        }
    }

    private async Task<int> CreatePlaylistAsync(PlaylistModel playlist)
    {
        var p = await FillDataAsync(playlist);

        return await _db.InsertAsync(p);
    }

    private async Task<int> UpdatePlaylistAsync(PlaylistModel playlist)
    {
        return await _db.UpdateAsync(playlist);
    }

    private void RemoveCache(string id = "")
    {
        _cache.Remove(CacheName);

        if (string.IsNullOrWhiteSpace(id) is false)
        {
            _cache.Remove(id);
        }
    }

    private static string GetCache(string id)
    {
        return $"{CacheName}-{id}";
    }

    private async Task<PlaylistModel> FillDataAsync(PlaylistModel playlist)
    {
        string defaultUrl = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";

        var channel = await _youtube.GetChannelAsync(playlist.AuthorUrl);
        string channelThumbnail = channel.Thumbnails.Count > 0 ? channel.Thumbnails[0].Url : defaultUrl;

        playlist.AuthorThumbnailUrl = channelThumbnail;

        return playlist;
    }
}
