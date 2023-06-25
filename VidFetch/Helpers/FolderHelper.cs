using VidFetchLibrary.Helpers;
using VidFetchLibrary.Library;

namespace VidFetch.Helpers;
public class FolderHelper : IFolderHelper
{
    private readonly ILauncher _launcher;
    private readonly IPathHelper _pathHelper;
    private readonly ISettingsLibrary _settings;

    public FolderHelper(ILauncher launcher,
                        IPathHelper pathHelper,
                        ISettingsLibrary settings)
    {
        _launcher = launcher;
        _pathHelper = pathHelper;
        _settings = settings;
    }

    public async Task OpenFolderLocationAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.SelectedPath))
        {
            return;
        }

        string folderPath = _pathHelper.OpenFolderLocation();

        await _launcher.OpenAsync(folderPath);
    }
}
