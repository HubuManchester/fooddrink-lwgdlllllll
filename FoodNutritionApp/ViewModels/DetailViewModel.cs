using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;

namespace FoodNutritionApp.ViewModels;

[QueryProperty(nameof(FoodItem), "FoodItem")]
public partial class DetailViewModel : BaseViewModel
{
    [ObservableProperty]
    private FoodItem _foodItem = new();

    [ObservableProperty]
    private bool _isSpeaking;

    public DetailViewModel()
    {
        Title = "Nutrition Details";
    }

    [RelayCommand]
    private async Task SpeakNutritionAsync()
    {
        if (IsSpeaking || string.IsNullOrWhiteSpace(FoodItem.Name))
        {
            return;
        }

        IsSpeaking = true;
        StatusMessage = "Reading nutrition information aloud...";

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                         ?? locales.FirstOrDefault();

            var options = new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f,
                Locale = locale
            };

            await TextToSpeech.Default.SpeakAsync(FoodItem.ToSpeechSummary(), options);
            StatusMessage = "Finished reading.";
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
        await Shell.Current.GoToAsync("..");
    }
}
