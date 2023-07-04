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
using YoutubeExplode.Videos;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using VidFetchLibrary.Cache;

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
        
        // YoutubeExplode clients
        builder.Services.AddTransient<YoutubeClient>();
        builder.Services.AddTransient<VideoClient>();
        builder.Services.AddTransient<ChannelClient>();
        builder.Services.AddTransient<PlaylistClient>();
        builder.Services.AddTransient<SearchClient>();

        // Caching
        builder.Services.AddMemoryCache();

        builder.Services.AddTransient<IVideoCache, VideoCache>();
        builder.Services.AddTransient<IChannelCache, ChannelCache>();
        builder.Services.AddTransient<IPlaylistCache, PlaylistCache>();
        builder.Services.AddTransient<IStreamInfoCache, StreamInfoCache>();

        // Video Library (Keeps the loaded data in)

        builder.Services.AddSingleton<IVideoLibrary, VideoLibrary>();

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

        builder.Services.AddSingleton<ILauncher, LauncherWrapper>();
    }
}
