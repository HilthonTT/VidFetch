namespace VidFetch.Pages;

public partial class SavedMedia
{
    private bool _isVisible = false;
    
    private void ToggleLoadingOverlay(bool isVisible)
    {
        _isVisible = isVisible;
    }
}