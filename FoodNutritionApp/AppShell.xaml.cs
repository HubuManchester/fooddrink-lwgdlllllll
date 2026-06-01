using FoodNutritionApp.Views;

namespace FoodNutritionApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(CameraPage), typeof(CameraPage));
        Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
        Routing.RegisterRoute(nameof(EditRecordPage), typeof(EditRecordPage));
        Routing.RegisterRoute(nameof(LocationPage), typeof(LocationPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
    }
}
