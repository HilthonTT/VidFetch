using SQLite;

namespace VidFetchLibrary.Models;
public class VideoModel
{
    [PrimaryKey]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string AuthorId { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Url { get; set; }
    public TimeSpan Duration { get; set; }
}
