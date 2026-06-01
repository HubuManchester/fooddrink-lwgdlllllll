using FoodNutritionApp.Models;

namespace FoodNutritionApp.Services;

/// <summary>
/// Dual-insurance data strategy: local bundled data first, mock API second.
/// </summary>
public class LocalFirstNutritionApi : INutritionApi
{
    private readonly LocalFoodDataService _localFoodData;
    private readonly MockNutritionApi _mockApi;

    public LocalFirstNutritionApi(LocalFoodDataService localFoodData)
    {
        _localFoodData = localFoodData;
        _mockApi = new MockNutritionApi();
    }

    public async Task<FoodItem?> SearchByNameAsync(string foodName, CancellationToken cancellationToken = default)
    {
        var local = await _localFoodData.SearchByNameAsync(foodName, cancellationToken);
        if (local != null)
        {
            return local;
        }

        var mock = await _mockApi.SearchByNameAsync(foodName, cancellationToken);
        if (mock != null)
        {
            mock.Source = "Mock API";
        }

        return mock;
    }

    public async Task<FoodItem?> RecognizeFromPhotoAsync(Stream photoStream, CancellationToken cancellationToken = default)
    {
        var local = await _localFoodData.RecognizeFromPhotoAsync(photoStream, cancellationToken);
        if (local != null)
        {
            return local;
        }

        var mock = await _mockApi.RecognizeFromPhotoAsync(photoStream, cancellationToken);
        if (mock != null)
        {
            mock.Source = "Mock API Camera";
        }

        return mock;
    }
}
