using CommunityToolkit.Maui;
using FoodNutritionApp.Services;
using FoodNutritionApp.ViewModels;
using FoodNutritionApp.Views;
using Microsoft.Extensions.Logging;

namespace FoodNutritionApp;

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

        // Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<INutritionApi>(sp => new RealNutritionApi(new HttpClient()));

#if ANDROID
        builder.Services.AddSingleton<ILightSensorService, Platforms.Android.AndroidLightSensorService>();
#else
        builder.Services.AddSingleton<ILightSensorService, DefaultLightSensorService>();
#endif

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<CameraViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<DetailViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();

        // Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CameraPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<HelpPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
