namespace VidFetch;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder.ConfigureServices();

		return builder.Build();
	}
}
