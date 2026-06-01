using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly LocalFoodDataService _localFoodData;
    private List<HistoryRecord> _allRecords = [];

    [ObservableProperty]
    private ObservableCollection<HistoryRecord> _records = [];

    [ObservableProperty]
    private string _selectedCategory = FoodCategories.All;

    public IList<string> CategoryOptions { get; } = FoodCategories.FilterOptions;

    public HistoryViewModel(DatabaseService databaseService, LocalFoodDataService localFoodData)
    {
        _databaseService = databaseService;
        _localFoodData = localFoodData;
        Title = "Food List";
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyCategoryFilter();
    }

    public async Task LoadRecordsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await _databaseService.InitializeAsync(_localFoodData);
            _allRecords = await _databaseService.GetAllRecordsAsync();
            ApplyCategoryFilter();
            StatusMessage = _allRecords.Count == 0
                ? "No saved records yet. Scan or search for food to build your list."
                : $"{Records.Count} of {_allRecords.Count} record(s) shown.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load records: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyCategoryFilter()
    {
        IEnumerable<HistoryRecord> filtered = _allRecords;
        if (SelectedCategory != FoodCategories.All)
        {
            filtered = _allRecords.Where(r =>
                r.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
        }

        Records = new ObservableCollection<HistoryRecord>(filtered);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadRecordsAsync();
    }

    [RelayCommand]
    private async Task OpenRecordAsync(HistoryRecord record)
    {
        if (record == null)
        {
            return;
        }

        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "FoodItem", record.ToFoodItem() }
        };

        await Shell.Current.GoToAsync(nameof(Views.DetailPage), navigationParameter);
    }

    [RelayCommand]
    private async Task EditRecordAsync(HistoryRecord record)
    {
        if (record == null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(Views.EditRecordPage)}?RecordId={record.Id}");
    }

    [RelayCommand]
    private async Task DeleteRecordAsync(HistoryRecord record)
    {
        if (record == null)
        {
            return;
        }

        try
        {
            await _databaseService.DeleteRecordAsync(record);
            _allRecords.Remove(record);
            Records.Remove(record);
            StatusMessage = "Record deleted.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not delete record: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ClearAllAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Clear list",
            "Delete all saved food records?",
            "Delete all",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        try
        {
            await _databaseService.ClearAllAsync();
            _allRecords.Clear();
            Records.Clear();
            StatusMessage = "All records cleared.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not clear records: {ex.Message}";
        }
    }
}
