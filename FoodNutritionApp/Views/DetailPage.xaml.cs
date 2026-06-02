using FoodNutritionApp.ViewModels;

namespace FoodNutritionApp.Views;

public partial class DetailPage : ContentPage
{
    private readonly DetailViewModel _viewModel;

    public DetailPage(DetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopSpeech();
    }
}
