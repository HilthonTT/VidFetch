using Newtonsoft.Json;
using SQLite;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace VidFetchLibrary.Models;
public class VideoModel
{
    private const string DefaultThumbnail = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";

    [AutoIncrement, PrimaryKey]
    public int Id { get; set; }
    public string VideoId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string AuthorName { get; set; }
    public string AuthorUrl { get; set; }
    public string AuthorThumbnailUrl { get; set; }
    public string ThumbnailUrl { get; set; }
    public string KeywordsJson
    {
        get => JsonConvert.SerializeObject(Keywords);
        set => Keywords = JsonConvert.DeserializeObject<List<string>>(value);
    }
    [Ignore]
    public List<string> Keywords { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTimeOffset UploadDate { get; set; }

    public VideoModel()
    {

    }

    public VideoModel(Video video)
    {
        VideoId = video.Id;
        Title = video.Title;
        Description = video.Description;
        Url = video.Url;
        AuthorName = video.Author.ChannelTitle;
        AuthorUrl = video.Author.ChannelUrl;
        AuthorThumbnailUrl = "";
        ThumbnailUrl = GetThumbnailSource(video.Thumbnails[0].Url);
        Keywords = video.Keywords.ToList();
        Duration = video.Duration.GetValueOrDefault();
        UploadDate = video.UploadDate;
    }

    public VideoModel(PlaylistVideo video)
    {
        VideoId = video.Id;
        Title = video.Title;
        Description = "";
        Url = video.Url;
        AuthorName = video.Author.ChannelTitle;
        AuthorUrl = video.Author.ChannelUrl;
        AuthorThumbnailUrl = "";
        ThumbnailUrl = GetThumbnailSource(video.Thumbnails[0].Url);
        Duration = video.Duration.GetValueOrDefault();
    }

    public VideoModel(VideoSearchResult video)
    {
        VideoId = video.Id;
        Title = video.Title;
        Description = "";
        Url = video.Url;
        AuthorName = video.Author.ChannelTitle;
        AuthorUrl = video.Author.ChannelUrl;
        AuthorThumbnailUrl = "";
        ThumbnailUrl = GetThumbnailSource(video.Thumbnails[0].Url);
        Duration = video.Duration.GetValueOrDefault();
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
