namespace FoodNutritionApp.Services;

/// <summary>
/// Speech-to-text for food name search (voice input).
/// </summary>
public interface ISpeechRecognitionService
{
    /// <summary>
    /// Returns recognized text, or null if nothing was heard / user cancelled.
  /// </summary>
    Task<string?> RecognizeFoodNameAsync(CancellationToken cancellationToken = default);
}
