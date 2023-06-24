namespace VidFetch.Services;
public class LauncherWrapper : ILauncher
{
    public Task<bool> CanOpenAsync(Uri uri)
    {
        return Launcher.CanOpenAsync(uri);
    }

    public Task<bool> OpenAsync(Uri uri)
    {
        return Launcher.OpenAsync(uri);
    }

    public Task<bool> OpenAsync(OpenFileRequest request)
    {
        return Launcher.OpenAsync(request);
    }

    public Task<bool> TryOpenAsync(Uri uri)
    {
        return Launcher.TryOpenAsync(uri);
    }
}
