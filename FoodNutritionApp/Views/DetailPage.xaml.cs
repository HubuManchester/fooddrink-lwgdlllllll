using FoodNutritionApp.ViewModels;

namespace FoodNutritionApp.Views;

public partial class DetailPage : ContentPage
{
    public DetailPage(DetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
