namespace VidFetchLibrary.Data;

public interface IDefaultData
{
    List<string> GetDownloadPaths();
    List<string> GetVideoExtensions();
    List<string> GetVideoResolutions();
}