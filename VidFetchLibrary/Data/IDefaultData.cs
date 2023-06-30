namespace VidFetchLibrary.Data;

public interface IDefaultData
{
    List<DownloadPath> GetDownloadPaths();
    List<VideoExtension> GetVideoExtensions();
    List<VideoResolution> GetVideoResolutions();
}