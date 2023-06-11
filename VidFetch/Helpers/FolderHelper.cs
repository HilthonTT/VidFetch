using VidFetchLibrary.Helpers;

namespace VidFetch.Helpers;
public class FolderHelper : IFolderHelper
{
    private readonly IPathHelper _pathHelper;

    public FolderHelper(IPathHelper pathHelper)
    {
        _pathHelper = pathHelper;
    }

    public async Task OpenFolderLocationAsync(string path)
    {
        string folderPath = _pathHelper.GetVideoDownloadPath("", "", path);

        await Launcher.OpenAsync(folderPath);
    }
}
