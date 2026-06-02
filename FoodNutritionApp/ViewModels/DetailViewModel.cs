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
        if (IsSpeaking || _speech.IsSpeaking || string.IsNullOrWhiteSpace(FoodItem.Name))
        {
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
        catch (Exception ex)
        {
            StatusMessage = $"Text-to-speech is unavailable: {ex.Message}";
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
