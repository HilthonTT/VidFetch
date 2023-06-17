using Newtonsoft.Json;
using SQLite;

namespace VidFetchLibrary.Models;
public class VideoModel
{
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
}
