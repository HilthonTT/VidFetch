using VidFetchLibrary.Helpers;

namespace VidFetch.Helpers;
public class FolderHelper : IFolderHelper
{
    private readonly ILauncher _launcher;
    private readonly IPathHelper _pathHelper;

    public FolderHelper(ILauncher launcher,
                        IPathHelper pathHelper)
    {
        _launcher = launcher;
        _pathHelper = pathHelper;
    }

    public async Task OpenFolderLocationAsync()
    {
        string folderPath = await _pathHelper.OpenFolderLocation();

        await _launcher.OpenAsync(folderPath);
    }
}
