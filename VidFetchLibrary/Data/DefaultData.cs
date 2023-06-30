namespace VidFetchLibrary.Data;

public class DefaultData : IDefaultData
{
    public List<VideoExtension> GetVideoExtensions()
    {
        return new List<VideoExtension>()
            {
                VideoExtension.Mp4,
                VideoExtension.Mp3,
                VideoExtension.Avi,
                VideoExtension.Mov,
                VideoExtension.Wmv,
                VideoExtension.Mkv,
                VideoExtension.Flv,
                VideoExtension.Tgpp,
                VideoExtension.Webm,
                VideoExtension.Mpg,
                VideoExtension.M4v,
                VideoExtension._3gp
            };
    }

    public List<DownloadPath> GetDownloadPaths()
    {
        return new List<DownloadPath>()
            {
                DownloadPath.DownloadFolder,
                DownloadPath.PictureFolder,
                DownloadPath.DocumentFolder,
                DownloadPath.VideoFolder,
                DownloadPath.MusicFolder,
                DownloadPath.Desktop
            };
    }

    public List<VideoResolution> GetVideoResolutions()
    {
        return new List<VideoResolution>()
            {
                VideoResolution.HighestResolution,
                VideoResolution.P144,
                VideoResolution.P240,
                VideoResolution.P360,
                VideoResolution.P480,
                VideoResolution.P720,
                VideoResolution.P1080,
                VideoResolution.P1440,
                VideoResolution.P2160
            };
    }
}

public enum VideoExtension
{
    Mp4,
    Mp3,
    Avi,
    Mov,
    Wmv,
    Mkv,
    Flv,
    Tgpp,
    Webm,
    Mpg,
    M4v,
    _3gp
}

public enum DownloadPath
{
    DownloadFolder,
    PictureFolder,
    DocumentFolder,
    VideoFolder,
    MusicFolder,
    Desktop
}

public enum VideoResolution
{
    HighestResolution,
    P144,
    P240,
    P360,
    P480,
    P720,
    P1080,
    P1440,
    P2160
}