using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

[QueryProperty(nameof(RecordId), "RecordId")]
public partial class EditRecordViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private int _recordId;

    [ObservableProperty]
    private string _foodName = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "Other";

    [ObservableProperty]
    private string _caloriesText = string.Empty;

    [ObservableProperty]
    private string _proteinText = string.Empty;

    [ObservableProperty]
    private string _fatText = string.Empty;

    [ObservableProperty]
    private string _carbohydratesText = string.Empty;

    [ObservableProperty]
    private string _fiberText = string.Empty;

    [ObservableProperty]
    private string _servingSize = "100g";

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public IList<string> Categories { get; } = FoodCategories.EditableOptions;

    public EditRecordViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Edit Record";
    }

    partial void OnRecordIdChanged(int value)
    {
        _ = LoadRecordAsync(value);
    }

    private async Task LoadRecordAsync(int id)
    {
        if (id <= 0)
        {
            return;
        }

        try
        {
            var record = await _databaseService.GetRecordByIdAsync(id);
            if (record == null)
            {
                StatusMessage = "Record not found.";
                return;
            }

            FoodName = record.FoodName;
            SelectedCategory = record.Category;
            CaloriesText = record.Calories.ToString("F0");
            ProteinText = record.Protein.ToString("F1");
            FatText = record.Fat.ToString("F1");
            CarbohydratesText = record.Carbohydrates.ToString("F1");
            FiberText = record.Fiber.ToString("F1");
            ServingSize = record.ServingSize;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load record: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidationMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FoodName))
        {
            ValidationMessage = "Food name is required.";
            return;
        }

        if (!double.TryParse(CaloriesText, out var calories) || calories < 0)
        {
            ValidationMessage = "Calories must be a valid non-negative number.";
            return;
        }

        if (!double.TryParse(ProteinText, out var protein) || protein < 0)
        {
            ValidationMessage = "Protein must be a valid non-negative number.";
            return;
        }

        if (!double.TryParse(FatText, out var fat) || fat < 0)
        {
            ValidationMessage = "Fat must be a valid non-negative number.";
            return;
        }

        if (!double.TryParse(CarbohydratesText, out var carbs) || carbs < 0)
        {
            ValidationMessage = "Carbohydrates must be a valid non-negative number.";
            return;
        }

        if (!double.TryParse(FiberText, out var fiber) || fiber < 0)
        {
            ValidationMessage = "Fiber must be a valid non-negative number.";
            return;
        }

        try
        {
            var record = await _databaseService.GetRecordByIdAsync(RecordId);
            if (record == null)
            {
                StatusMessage = "Record not found.";
                return;
            }

            record.FoodName = FoodName.Trim();
            record.Category = SelectedCategory;
            record.Calories = calories;
            record.Protein = protein;
            record.Fat = fat;
            record.Carbohydrates = carbs;
            record.Fiber = fiber;
            record.ServingSize = string.IsNullOrWhiteSpace(ServingSize) ? "100g" : ServingSize.Trim();

            await _databaseService.UpdateRecordAsync(record);
            StatusMessage = "Record updated successfully.";
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save changes: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
