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

        builder.Services.AddSingleton<AppSettingsService>();
        builder.Services.AddSingleton<LocalFoodDataService>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<MockNutritionApi>();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<INutritionApi, HybridNutritionApi>();

#if WINDOWS
        builder.Services.AddSingleton<ISpeechPlaybackService, Platforms.Windows.WindowsSpeechPlaybackService>();
#else
        builder.Services.AddSingleton<ISpeechPlaybackService, MauiTextToSpeechPlaybackService>();
#endif

#if ANDROID
        builder.Services.AddSingleton<ILightSensorService, Platforms.Android.AndroidLightSensorService>();
#else
        builder.Services.AddSingleton<ILightSensorService, DefaultLightSensorService>();
#endif

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<CameraViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<DetailViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<EditRecordViewModel>();
        builder.Services.AddTransient<LocationViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CameraPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<EditRecordPage>();
        builder.Services.AddTransient<LocationPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<HelpPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        app.Services.GetRequiredService<AppSettingsService>().Initialize();
        return app;
    }
}
