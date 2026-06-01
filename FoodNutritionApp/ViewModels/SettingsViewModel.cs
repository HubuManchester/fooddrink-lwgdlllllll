using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly AppSettingsService _appSettings;

    [ObservableProperty]
    private AppThemeMode _selectedTheme;

    [ObservableProperty]
    private double _selectedFontScale = 1.0;

    public IList<string> ThemeOptions { get; } =
    [
        "Follow System",
        "Light",
        "Dark"
    ];

    public IList<string> FontScaleOptions { get; } =
    [
        "Normal",
        "Large",
        "Extra Large"
    ];

    [ObservableProperty]
    private string _selectedThemeLabel = "Follow System";

    [ObservableProperty]
    private string _selectedFontScaleLabel = "Normal";

    public SettingsViewModel(AppSettingsService appSettings)
    {
        _appSettings = appSettings;
        Title = "Settings";
        StatusMessage = "Theme follows system by default. Large text improves readability.";

        SelectedTheme = _appSettings.ThemeMode;
        SelectedFontScale = _appSettings.FontScale;
        SelectedThemeLabel = ToThemeLabel(SelectedTheme);
        SelectedFontScaleLabel = ToFontLabel(SelectedFontScale);
    }

    [RelayCommand]
    private void SelectTheme(string label)
    {
        SelectedThemeLabel = label;
        SelectedTheme = label switch
        {
            "Light" => AppThemeMode.Light,
            "Dark" => AppThemeMode.Dark,
            _ => AppThemeMode.System
        };
        _appSettings.ThemeMode = SelectedTheme;
        StatusMessage = $"Theme set to {label}.";
    }

    [RelayCommand]
    private void SelectFontScale(string label)
    {
        SelectedFontScaleLabel = label;
        SelectedFontScale = label switch
        {
            "Large" => 1.25,
            "Extra Large" => 1.5,
            _ => 1.0
        };
        _appSettings.FontScale = SelectedFontScale;
        StatusMessage = $"Text size set to {label}.";
    }

    private static string ToThemeLabel(AppThemeMode mode) => mode switch
    {
        AppThemeMode.Light => "Light",
        AppThemeMode.Dark => "Dark",
        _ => "Follow System"
    };

    private static string ToFontLabel(double scale) => scale switch
    {
        >= 1.45 => "Extra Large",
        >= 1.2 => "Large",
        _ => "Normal"
    };
}
