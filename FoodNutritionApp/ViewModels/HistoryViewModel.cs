using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<HistoryRecord> _records = [];

    public HistoryViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "History";
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
            var items = await _databaseService.GetAllRecordsAsync();
            Records = new ObservableCollection<HistoryRecord>(items);
            StatusMessage = Records.Count == 0
                ? "No saved records yet. Scan or search for food to build your history."
                : $"{Records.Count} record(s) saved locally.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load history: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
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
    private async Task DeleteRecordAsync(HistoryRecord record)
    {
        if (record == null)
        {
            return;
        }

        try
        {
            await _databaseService.DeleteRecordAsync(record);
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
            "Clear history",
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
            Records.Clear();
            StatusMessage = "All history cleared.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not clear history: {ex.Message}";
        }
    }
}
