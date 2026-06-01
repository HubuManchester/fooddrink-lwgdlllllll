using CommunityToolkit.Mvvm.ComponentModel;

namespace FoodNutritionApp.ViewModels;

/// <summary>
/// Base class for all ViewModels with busy state and title support.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;
}
