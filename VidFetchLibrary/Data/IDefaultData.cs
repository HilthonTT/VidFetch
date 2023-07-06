namespace VidFetchLibrary.Data;

public interface IDefaultData
{
    List<DownloadPath> GetDownloadPaths();
    List<Language> GetLanguages();
    List<VideoExtension> GetVideoExtensions();
    List<VideoResolution> GetVideoResolutions();
}