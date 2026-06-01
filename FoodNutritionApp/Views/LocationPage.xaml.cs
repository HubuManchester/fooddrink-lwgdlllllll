using FoodNutritionApp.ViewModels;

namespace FoodNutritionApp.Views;

public partial class LocationPage : ContentPage
{
    public LocationPage(LocationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
