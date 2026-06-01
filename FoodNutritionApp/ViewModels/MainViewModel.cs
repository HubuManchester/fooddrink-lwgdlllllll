using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FoodNutritionApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    public MainViewModel()
    {
        Title = "Food Nutrition";
        StatusMessage = "Scan, search, or review your food history.";
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
    private async Task GoToHelpAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.HelpPage));
    }
}
