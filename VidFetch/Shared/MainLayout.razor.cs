using VidFetchLibrary.Library;
using VidFetchLibrary.Data;

namespace VidFetch.Shared;

public partial class MainLayout
{
    private SettingsLibrary _settings;
    private bool _isDarkMode = true;

    protected override async Task OnInitializedAsync()
    {
        _settings = await settingsData.GetSettingsAsync();

        if (_settings is not null)
        {
            _isDarkMode = _settings.IsDarkMode;
        }
        else
        {
            _isDarkMode = true;
        }
    }
}