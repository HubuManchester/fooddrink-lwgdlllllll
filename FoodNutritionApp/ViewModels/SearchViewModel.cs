using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly INutritionApi _nutritionApi;
    private readonly DatabaseService _databaseService;
    private readonly ISpeechRecognitionService _speechRecognition;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _isListening;

    public SearchViewModel(
        INutritionApi nutritionApi,
        DatabaseService databaseService,
        ISpeechRecognitionService speechRecognition)
    {
        _nutritionApi = nutritionApi;
        _databaseService = databaseService;
        _speechRecognition = speechRecognition;
        Title = "Search Food";
    }

    partial void OnIsListeningChanged(bool value) => VoiceInputCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private async Task SearchAsync()
    {
        ValidationMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            ValidationMessage = "Please enter a food name before searching.";
            return;
        }

        if (SearchText.Trim().Length < 2)
        {
            ValidationMessage = "Food name must be at least 2 characters long.";
            return;
        }

        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        VoiceInputCommand.NotifyCanExecuteChanged();

        try
        {
            var result = await _nutritionApi.SearchByNameAsync(SearchText.Trim());

            if (result == null)
            {
                StatusMessage = $"No nutrition data found for \"{SearchText.Trim()}\". Try apple, banana, chicken, or rice.";
                return;
            }

            await _databaseService.SaveRecordAsync(result);

            var navigationParameter = new ShellNavigationQueryParameters
            {
                { "FoodItem", result }
            };

            await Shell.Current.GoToAsync(nameof(Views.DetailPage), navigationParameter);
        }
        catch (HttpRequestException)
        {
            StatusMessage = "Network error. Please check your internet connection and try again.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to complete search: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            VoiceInputCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanUseVoiceInput => !IsBusy && !IsListening;

    [RelayCommand(CanExecute = nameof(CanUseVoiceInput))]
    private async Task VoiceInputAsync()
    {
        if (IsBusy || IsListening)
        {
            return;
        }

        ValidationMessage = string.Empty;
        IsListening = true;
        StatusMessage = "Checking microphone permission...";

        try
        {
            if (!await SpeechPermissionHelper.EnsureMicrophoneAsync())
            {
                StatusMessage = "Microphone permission denied. Open Settings → Apps → FoodNutritionApp → Permissions → Microphone.";
                return;
            }

            StatusMessage = "Listening... Say a food name in English (e.g. apple, rice). A system speech dialog will open.";

            var recognizedText = await _speechRecognition.RecognizeFoodNameAsync(CancellationToken.None);
            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                StatusMessage = "No speech recognized. Install or update Google and Speech Services by Google, stay online, and try saying \"apple\" in English.";
                return;
            }

            SearchText = recognizedText.Trim();
            StatusMessage = $"Recognized: {SearchText}";
            await SearchAsync();
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Speech recognition is not available on this device. Install the Google app and Speech Services by Google, then enable them in system settings.";
        }
        catch (PermissionException)
        {
            StatusMessage = "Speech recognition permission was denied. Allow microphone access for this app.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Voice input failed: {ex.Message}";
        }
        finally
        {
            IsListening = false;
            VoiceInputCommand.NotifyCanExecuteChanged();
        }
    }
}
