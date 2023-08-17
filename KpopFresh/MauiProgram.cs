using Microsoft.Extensions.Logging;
using KpopFresh.ViewModel;
using KpopFresh.Services;
using CommunityToolkit.Maui;

namespace KpopFresh;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        builder.Services.AddSingleton<SongsViewModel>();
        builder.Services.AddTransient<SongsViewModel>();
        builder.Services.AddSingleton<WebScraper>();
        builder.Services.AddTransient<MainPage>();

		#if DEBUG
				builder.Logging.AddDebug();
		#endif

		return builder.Build();
	}
}
