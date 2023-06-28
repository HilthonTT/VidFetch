namespace VidFetchLibrary.Data;

public class DefaultData : IDefaultData
{
    public List<string> GetVideoExtensions()
    {
        return new List<string>()
        {
            ".mp4",
            ".mp3",
            ".avi",
            ".mov",
            ".wmv",
            ".mkv",
            ".flv",
            ".tgpp",
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

    public List<string> GetVideoResolutions()
    {
        return new List<string>()
        {
            "Highest Resolution",
            "144p",
            "240p",
            "360p",
            "480p",
            "720p",
            "1080p",
            "1440p",
            "2160p",
        };
    }
}
