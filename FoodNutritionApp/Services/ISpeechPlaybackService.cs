namespace FoodNutritionApp.Services;

/// <summary>
/// Cross-platform speech playback with reliable stop support (especially on Windows).
/// </summary>
public interface ISpeechPlaybackService
{
    bool IsSpeaking { get; }

    Task SpeakAsync(string text, CancellationToken cancellationToken = default);

    void Stop();
}
