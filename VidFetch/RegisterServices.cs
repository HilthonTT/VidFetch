using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using VidFetch.Helpers;
using VidFetch.Services;
using VidFetchLibrary.Data;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Client;
using VidFetchLibrary.Helpers;
using VidFetchLibrary.Library;

namespace VidFetch;
public static class RegisterServices
{
    public static void ConfigureServices(this MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddMudServices();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IVideoLibrary, VideoLibrary>();
        builder.Services.AddSingleton<ISettingsLibrary, SettingsLibrary>();

        // Helpers
        builder.Services.AddSingleton<ISearchHelper,  SearchHelper>();
        builder.Services.AddSingleton<ITokenHelper, TokenHelper>();
        builder.Services.AddSingleton<IVideoLibraryHelper, VideoLibraryHelper>();
        builder.Services.AddSingleton<IFolderHelper, FolderHelper>();

        // Data Access
        builder.Services.AddSingleton<ISettingsData, SettingsData>();
        builder.Services.AddSingleton<IVideoData, VideoData>();
        builder.Services.AddSingleton<IChannelData, ChannelData>();
        builder.Services.AddSingleton<IPlaylistData, PlaylistData>();

        // Personal Services
        builder.Services.AddSingleton<IDefaultData, DefaultData>();
        builder.Services.AddSingleton<IYoutube, Youtube>();
        builder.Services.AddSingleton<IDownloadHelper, DownloadHelper>();
        builder.Services.AddSingleton<IPathHelper, PathHelper>();
        builder.Services.AddSingleton<ICachingHelper, CachingHelper>();

        builder.Services.AddSingleton<ISecureStorage, SecureStorageWrapper>();
    }
}
