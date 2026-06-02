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

        UpdateThemeResources();
        RefreshAllPageBackgrounds();
    }

    private static void UpdateThemeResources()
    {
        if (Application.Current?.Resources == null)
        {
            return;
        }

        var isDark = GetEffectiveTheme() == AppTheme.Dark;
        var resources = Application.Current.Resources;

        resources["Background"] = Color.FromArgb(isDark ? "#121212" : "#FAFAFA");
        resources["Surface"] = Color.FromArgb(isDark ? "#1E1E1E" : "#FFFFFF");
        resources["TextPrimary"] = Color.FromArgb(isDark ? "#F5F5F5" : "#1B1B1B");
        resources["TextSecondary"] = Color.FromArgb(isDark ? "#BDBDBD" : "#424242");
        resources["SectionTitle"] = Color.FromArgb(isDark ? "#A5D6A7" : "#1B5E20");
        resources["ShellBackground"] = Color.FromArgb(isDark ? "#1B5E20" : "#2E7D32");
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

    private static void RefreshAllPageBackgrounds()
    {
        if (Application.Current == null)
        {
            return;
        }

        var background = ResolveThemeColor("Background");
        var shellBg = ResolveThemeColor("ShellBackground");

        foreach (var window in Application.Current.Windows)
        {
            if (window.Page == null)
            {
                continue;
            }

            RefreshPageTree(window.Page, background, shellBg);
        }
    }

    private static void RefreshPageTree(Page? page, Color background, Color? shellBg = null)
    {
        if (page == null)
        {
            return;
        }

        if (page is Shell shell)
        {
            if (shellBg != null)
            {
                shell.BackgroundColor = shellBg;
            }

            foreach (var stackPage in shell.Navigation.NavigationStack)
            {
                RefreshPageTree(stackPage, background);
            }

            if (shell.CurrentPage != null)
            {
                RefreshPageTree(shell.CurrentPage, background);
            }

            return;
        }

        if (page is ContentPage contentPage)
        {
            contentPage.BackgroundColor = background;
        }

        if (page is NavigationPage nav)
        {
            foreach (var stackPage in nav.Navigation.NavigationStack)
            {
                RefreshPageTree(stackPage, background);
            }

            foreach (var modalPage in nav.Navigation.ModalStack)
            {
                RefreshPageTree(modalPage, background);
            }
        }

        if (page is TabbedPage tabbed)
        {
            foreach (var child in tabbed.Children.OfType<Page>())
            {
                RefreshPageTree(child, background);
            }
        }

        if (page is FlyoutPage flyout)
        {
            RefreshPageTree(flyout.Detail, background);
            RefreshPageTree(flyout.Flyout, background);
        }
    }

    private static Color ResolveThemeColor(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
        {
            return color;
        }

        var isDark = GetEffectiveTheme() == AppTheme.Dark;
        return isDark ? Color.FromArgb("#121212") : Color.FromArgb("#FAFAFA");
    }

    private static AppTheme GetEffectiveTheme()
    {
        if (Application.Current == null)
        {
            return AppTheme.Light;
        }

        return Application.Current.UserAppTheme == AppTheme.Unspecified
            ? Application.Current.RequestedTheme
            : Application.Current.UserAppTheme;
    }
}

public enum AppThemeMode
{
    System,
    Light,
    Dark
}
