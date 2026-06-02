using FoodNutritionApp.Models;

namespace FoodNutritionApp.Services;

/// <summary>
/// Nutrition lookup chain: bundled local data → USDA API → mock API fallback.
/// </summary>
public class HybridNutritionApi : INutritionApi
{
    private readonly LocalFoodDataService _localFoodData;
    private readonly RealNutritionApi _realApi;
    private readonly MockNutritionApi _mockApi;

    public HybridNutritionApi(
        LocalFoodDataService localFoodData,
        HttpClient httpClient,
        MockNutritionApi mockApi)
    {
        _localFoodData = localFoodData;
        _mockApi = mockApi;
        _realApi = new RealNutritionApi(httpClient);
    }

    public async Task<FoodItem?> SearchByNameAsync(string foodName, CancellationToken cancellationToken = default)
    {
        var local = await _localFoodData.SearchByNameAsync(foodName, cancellationToken);
        if (local != null)
        {
            local.Source = "Local Database";
            return local;
        }

        var remote = await _realApi.SearchByNameAsync(foodName, cancellationToken);
        if (remote != null)
        {
            return remote;
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
            local.Source = "Local Camera";
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
