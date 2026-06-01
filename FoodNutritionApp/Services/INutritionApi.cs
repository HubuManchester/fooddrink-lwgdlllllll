using FoodNutritionApp.Models;

namespace FoodNutritionApp.Services;

/// <summary>
/// Contract for fetching food nutrition data from a remote or mock source.
/// </summary>
public interface INutritionApi
{
    Task<FoodItem?> SearchByNameAsync(string foodName, CancellationToken cancellationToken = default);
    Task<FoodItem?> RecognizeFromPhotoAsync(Stream photoStream, CancellationToken cancellationToken = default);
}
