using FoodNutritionApp.ViewModels;

namespace FoodNutritionApp.Views;

public partial class CameraPage : ContentPage
{
    public CameraPage(CameraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
