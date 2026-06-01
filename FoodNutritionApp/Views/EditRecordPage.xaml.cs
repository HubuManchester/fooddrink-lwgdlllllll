using FoodNutritionApp.ViewModels;

namespace FoodNutritionApp.Views;

public partial class EditRecordPage : ContentPage
{
    public EditRecordPage(EditRecordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
