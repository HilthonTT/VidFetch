using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using VidFetch.Helpers;
using VidFetch.Services;
using VidFetchLibrary.Data;
using VidFetchLibrary.DataAccess;
using VidFetchLibrary.Client;
using VidFetchLibrary.Helpers;
using VidFetchLibrary.Library;
using YoutubeExplode;
using VidFetchLibrary.Cache;
using VidFetchLibrary.Language;
using VidFetch.Generics;

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

        // Generics
        builder.Services.AddScoped(typeof(IGenericDeleteHelper<>), typeof(GenericDeleteHelper<>));
        builder.Services.AddScoped(typeof(IGenericDownloadHelper<>), typeof(GenericDownloadHelper<>));
        builder.Services.AddScoped(typeof(IGenericLoadHelper<>), typeof(GenericLoadHelper<>));
        builder.Services.AddScoped(typeof(IGenericSaveHelper<>), typeof(GenericSaveHelper<>));
        builder.Services.AddScoped(typeof(IGenericUpdateHelper<>), typeof(GenericUpdateHelper<>));
        builder.Services.AddScoped(typeof(IGenericGeneralHelper<>), typeof(GenericGeneralHelper<>));
        
        // YoutubeExplode client
        builder.Services.AddScoped<YoutubeClient>();

        // Language
        builder.Services.AddScoped<ILanguageExtension, LanguageExtension>();
        builder.Services.AddScoped<IEnglishDictionary, EnglishDictionary>();
        builder.Services.AddScoped<IFrenchDictionary, FrenchDictionary>();
        builder.Services.AddScoped<IIndonesianDictionary, IndonesianDictionary>();

        // Caching
        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<IVideoCache, VideoCache>();
        builder.Services.AddScoped<IChannelCache, ChannelCache>();
        builder.Services.AddScoped<IPlaylistCache, PlaylistCache>();
        builder.Services.AddScoped<IStreamInfoCache, StreamInfoCache>();
        builder.Services.AddScoped<ILoadedItemsCache,  LoadedItemsCache>();

        // Video Library (Keeps the loaded data in)
        builder.Services.AddScoped<IVideoLibrary, VideoLibrary>();

        // Helpers
        builder.Services.AddScoped(typeof(ISearchHelper<>), typeof(SearchHelper<>));
        builder.Services.AddScoped(typeof(IVideoLibraryFilterHelper<>), typeof(VideoLibraryFilterHelper<>));
        builder.Services.AddScoped<ISnackbarHelper, SnackbarHelper>();

        builder.Services.AddScoped<ITokenHelper, TokenHelper>();
        builder.Services.AddScoped<IFolderHelper, FolderHelper>();

        // Data Access
        builder.Services.AddScoped<ISettingsData, SettingsData>();
        builder.Services.AddScoped<IVideoData, VideoData>();
        builder.Services.AddScoped<IChannelData, ChannelData>();
        builder.Services.AddScoped<IPlaylistData, PlaylistData>();

        // Personal Services
        builder.Services.AddScoped<IDefaultData, DefaultData>();
        builder.Services.AddScoped<IYoutube, Youtube>();
        builder.Services.AddScoped<IDownloadHelper, DownloadHelper>();
        builder.Services.AddScoped<IPathHelper, PathHelper>();

        builder.Services.AddScoped<ILauncher, LauncherWrapper>();
        builder.Services.AddScoped<IClipboard, ClipboardWrapper>();
    }
}
