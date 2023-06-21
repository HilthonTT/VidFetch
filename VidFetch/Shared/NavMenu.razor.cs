namespace VidFetch.Shared;

public partial class NavMenu
{
    private void LoadHomePage()
    {
        navManager.NavigateTo("/");
    }

    private void LoadDownloadPage()
    {
        navManager.NavigateTo("/Download");
    }

    private void LoadSettingsPage()
    {
        navManager.NavigateTo("/Settings");
    }

    private void LoadSavedMediasPage()
    {
        navManager.NavigateTo("/SavedMedias");
    }

    private void LoadSearchPage()
    {
        navManager.NavigateTo("/Search");
    }

    private void LoadGithubPage()
    {
        navManager.NavigateTo("https://github.com/HilthonTT");
    }
}