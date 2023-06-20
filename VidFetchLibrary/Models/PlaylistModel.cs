using SQLite;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;

namespace VidFetchLibrary.Models;
public class PlaylistModel
{
    private const string DefaultThumbnail = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";

    [AutoIncrement, PrimaryKey]
    public int Id { get; set; }
    public string PlaylistId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string ThumbnailUrl { get; set; }
    public string AuthorName { get; set; }
    public string AuthorUrl { get; set; }
    public string AuthorThumbnailUrl { get; set; }

    public PlaylistModel()
    {
        
    }

    public PlaylistModel(Playlist playlist)
    {
        PlaylistId = playlist.Id;
        Title = playlist.Title;
        Description = playlist.Description;
        Url = playlist.Url;
        ThumbnailUrl = GetThumbnailSource(playlist.Thumbnails[0].Url);
        AuthorName = playlist.Author.ChannelTitle;
        AuthorUrl = playlist.Author.ChannelUrl;
        AuthorThumbnailUrl = "";
    }

    public PlaylistModel(PlaylistSearchResult playlist)
    {
        PlaylistId = playlist.Id;
        Title = playlist.Title;
        Description = "";
        Url = playlist.Url;
        ThumbnailUrl = GetThumbnailSource(playlist.Thumbnails[0].Url);
        AuthorName = playlist.Author.ChannelTitle;
        AuthorUrl = playlist.Author.ChannelUrl;
        AuthorThumbnailUrl = "";
    }

    private static string GetThumbnailSource(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return DefaultThumbnail;
        }

        return source;
    }
}
