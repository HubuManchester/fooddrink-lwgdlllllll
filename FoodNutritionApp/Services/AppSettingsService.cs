namespace FoodNutritionApp.Services;

/// <summary>
/// Persists theme and font scale preferences and applies them app-wide.
/// </summary>
public class AppSettingsService
{
    private const string ThemeKey = "app_theme";
    private const string FontScaleKey = "font_scale";

    public event Action? SettingsChanged;

    public AppThemeMode ThemeMode
    {
        get => Enum.TryParse<AppThemeMode>(Preferences.Get(ThemeKey, AppThemeMode.System.ToString()), out var mode)
            ? mode
            : AppThemeMode.System;
        set
        {
            Preferences.Set(ThemeKey, value.ToString());
            ApplyTheme();
            UpdateFontResources();
            SettingsChanged?.Invoke();
        }
    }

    public double FontScale
    {
        get => Preferences.Get(FontScaleKey, 1.0);
        set
        {
            Preferences.Set(FontScaleKey, Math.Clamp(value, 1.0, 1.75));
            UpdateFontResources();
            SettingsChanged?.Invoke();
        }
    }

    public double BodyFontSize => 16 * FontScale;
    public double TitleFontSize => 24 * FontScale;
    public double SubtitleFontSize => 18 * FontScale;

    public void Initialize()
    {
        ApplyTheme();
        UpdateFontResources();
    }

    public void ApplyTheme()
    {
        if (Application.Current == null)
        {
            return;
        }

        Application.Current.UserAppTheme = ThemeMode switch
        {
            AppThemeMode.Light => AppTheme.Light,
            AppThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    public void UpdateFontResources()
    {
        if (Application.Current?.Resources == null)
        {
            return;
        }

        Application.Current.Resources["FontSizeBody"] = BodyFontSize;
        Application.Current.Resources["FontSizeTitle"] = TitleFontSize;
        Application.Current.Resources["FontSizeSubtitle"] = SubtitleFontSize;
    }
}

public enum AppThemeMode
{
    System,
    Light,
    Dark
}
