namespace VidFetchLibrary.Data;

public class DefaultData : IDefaultData
{
    public List<string> GetVideoExtensions()
    {
        return new List<string>()
        {
            ".mp4",
            ".avi",
            ".mov",
            ".wmv",
            ".mkv",
            ".flv",
            ".webm",
            ".mpg",
            ".m4v",
            ".3gp",
        };
    }

    public List<string> GetDownloadPaths()
    {
        return new List<string>()
        {
            "Download Folder",
            "Picture Folder",
            "Document Folder",
            "Video Folder",
            "Music Folder",
            "Desktop",
        };
    }
}
