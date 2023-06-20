using SQLite;
using YoutubeExplode.Channels;
using YoutubeExplode.Search;

namespace VidFetchLibrary.Models;
public class ChannelModel
{
    private const string DefaultThumbnail = "https://dummyimage.com/1200x900/000/ffffff&text=No+image+available.";

    [AutoIncrement, PrimaryKey]
    public int Id { get; set; }
    public string ChannelId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string ThumbnailUrl { get; set; }

    public ChannelModel()
    {
        
    }

    public ChannelModel(Channel channel)
    {
        ChannelId = channel.Id;
        Title = channel.Title;
        Url = channel.Url;
        ThumbnailUrl = GetThumbnailSource(channel.Thumbnails[0].Url);
    }

    public ChannelModel(ChannelSearchResult channel)
    {
        ChannelId = channel.Id;
        Title = channel.Title;
        Url = channel.Url;
        ThumbnailUrl = GetThumbnailSource(channel.Thumbnails[0].Url);
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
