using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

[QueryProperty(nameof(FoodItem), "FoodItem")]
public partial class DetailViewModel : BaseViewModel
{
    private readonly ISpeechPlaybackService _speech;

    [ObservableProperty]
    private FoodItem _foodItem = new();

    [ObservableProperty]
    private bool _isSpeaking;

    public DetailViewModel(ISpeechPlaybackService speech)
    {
        _speech = speech;
        Title = "Nutrition Details";
    }

    public void StopSpeech()
    {
        _speech.Stop();
        IsSpeaking = false;
        StatusMessage = "Reading stopped.";
    }

    /// <summary>
    /// Synchronous stop — can run while <see cref="StartReadingAsync"/> is still awaiting.
    /// </summary>
    [RelayCommand]
    private void StopReading()
    {
        StopSpeech();
    }

    [RelayCommand]
    private async Task StartReadingAsync()
    {
        if (IsSpeaking || _speech.IsSpeaking)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(FoodItem.Name))
        {
            StatusMessage = "No food data to read. Open a food item from search or the food list first.";
            return;
        }

        IsSpeaking = true;
        StatusMessage = "Reading nutrition information aloud... Tap stop to end.";

        try
        {
            await _speech.SpeakAsync(FoodItem.ToSpeechSummary());

            if (IsSpeaking)
            {
                StatusMessage = "Finished reading.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Reading stopped.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message.Contains("TTS", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("Text to Speech", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("Text-to-speech", StringComparison.OrdinalIgnoreCase)
                ? "Text-to-speech failed. Open Settings → System → Text-to-speech, select Google Text-to-speech, install English voice data, tap Listen to test, then raise Media volume."
                : $"Text-to-speech failed: {ex.Message}. Check Media volume and Text-to-speech in system settings.";
        }
        finally
        {
            IsSpeaking = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        StopSpeech();
        await Shell.Current.GoToAsync("..");
    }
}
