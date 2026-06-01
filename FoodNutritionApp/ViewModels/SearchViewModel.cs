using System.Globalization;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly INutritionApi _nutritionApi;
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public SearchViewModel(INutritionApi nutritionApi, DatabaseService databaseService)
    {
        _nutritionApi = nutritionApi;
        _databaseService = databaseService;
        Title = "Search Food";
    }

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
        }
    }

    [RelayCommand]
    private async Task VoiceInputAsync()
    {
        ValidationMessage = string.Empty;
        StatusMessage = "Listening... Say a food name such as apple or banana.";

        try
        {
            var isGranted = await SpeechToText.Default.RequestPermissions(CancellationToken.None);
            if (!isGranted)
            {
                StatusMessage = "Microphone permission is required for voice input.";
                return;
            }

            var recognizedText = await ListenForSpeechAsync();
            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                StatusMessage = "No speech detected. Please try again.";
                return;
            }

            SearchText = recognizedText;
            StatusMessage = $"Recognized: {recognizedText}";
            await SearchAsync();
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Speech recognition is not supported on this device.";
        }
        catch (PermissionException)
        {
            StatusMessage = "Microphone permission was denied.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Voice input failed: {ex.Message}";
        }
    }

    private static async Task<string?> ListenForSpeechAsync()
    {
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
        {
            Cleanup();
            tcs.TrySetResult(e.RecognitionResult?.Text);
        }

        void OnUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.RecognitionResult))
            {
                Cleanup();
                tcs.TrySetResult(e.RecognitionResult);
            }
        }

        void Cleanup()
        {
            SpeechToText.Default.RecognitionResultCompleted -= OnCompleted;
            SpeechToText.Default.RecognitionResultUpdated -= OnUpdated;
        }

        SpeechToText.Default.RecognitionResultCompleted += OnCompleted;
        SpeechToText.Default.RecognitionResultUpdated += OnUpdated;

        try
        {
            var options = new SpeechToTextOptions
            {
                Culture = CultureInfo.CurrentCulture
            };

            await SpeechToText.Default.StartListenAsync(options, CancellationToken.None);

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(12)));
            if (completedTask != tcs.Task)
            {
                return null;
            }

            return await tcs.Task;
        }
        finally
        {
            Cleanup();

            try
            {
                await SpeechToText.Default.StopListenAsync(CancellationToken.None);
            }
            catch
            {
                // Ignore stop errors when recognition already ended.
            }
        }
    }
}
