using SQLite;

namespace VidFetchLibrary.Models;
public class AuthorModel
{
    [PrimaryKey]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
}
