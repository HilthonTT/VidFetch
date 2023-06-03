using Microsoft.Extensions.Logging;
using VidFetch.Data;
using VidFetchLibrary.Data;
using VidFetchLibrary.Downloader;
using VidFetchLibrary.Helpers;

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

        builder.Services.AddSingleton<IDefaultData, DefaultData>();
        builder.Services.AddSingleton<IYoutubeDownloader, YoutubeDownloader>();
        builder.Services.AddSingleton<IDownloadHelper, DownloadHelper>();
        builder.Services.AddSingleton<IPathHelper, PathHelper>();

        
        builder.Services.AddSingleton<WeatherForecastService>();
    }
}
