using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FoodNutritionApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    public MainViewModel()
    {
        Title = "Food Nutrition";
        StatusMessage = "Scan, search, browse your food list, or check location.";
    }

    [RelayCommand]
    private async Task GoToCameraAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.CameraPage));
    }

    [RelayCommand]
    private async Task GoToSearchAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.SearchPage));
    }

    [RelayCommand]
    private async Task GoToHistoryAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.HistoryPage));
    }

    [RelayCommand]
    private async Task GoToLocationAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.LocationPage));
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
    }

    [RelayCommand]
    private async Task GoToHelpAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.HelpPage));
    }
}
