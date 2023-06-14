namespace VidFetch.Shared;

public partial class NavMenu
{
    private void LoadHomePage()
    {
        navManager.NavigateTo("/");
    }

    private void LoadSettingsPage()
    {
        navManager.NavigateTo("/Settings");
    }

    private void LoadSavedMedias()
    {
        navManager.NavigateTo("/SavedMedias");
    }

    private void LoadGithubPage()
    {
        navManager.NavigateTo("https://github.com/HilthonTT");
    }
}