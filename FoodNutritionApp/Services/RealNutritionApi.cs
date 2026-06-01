using System.Net.Http.Json;
using System.Text.Json;
using FoodNutritionApp.Models;

namespace FoodNutritionApp.Services;

/// <summary>
/// Optional real API using USDA FoodData Central (free, no key required for demo endpoint).
/// Falls back gracefully when the network is unavailable.
/// </summary>
public class RealNutritionApi : INutritionApi
{
    private readonly HttpClient _httpClient;
    private readonly MockNutritionApi _fallback;

    public RealNutritionApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _fallback = new MockNutritionApi();
    }

    public async Task<FoodItem?> SearchByNameAsync(string foodName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(foodName))
        {
            return null;
        }

        try
        {
            var url = $"https://api.nal.usda.gov/fdc/v1/foods/search?query={Uri.EscapeDataString(foodName.Trim())}&pageSize=1&api_key=DEMO_KEY";
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await _fallback.SearchByNameAsync(foodName, cancellationToken);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("foods", out var foods) || foods.GetArrayLength() == 0)
            {
                return await _fallback.SearchByNameAsync(foodName, cancellationToken);
            }

            var food = foods[0];
            var description = food.GetProperty("description").GetString() ?? foodName.Trim();
            var nutrients = food.GetProperty("foodNutrients");

            return new FoodItem
            {
                Name = description,
                Calories = GetNutrient(nutrients, 1008),
                Protein = GetNutrient(nutrients, 1003),
                Fat = GetNutrient(nutrients, 1004),
                Carbohydrates = GetNutrient(nutrients, 1005),
                Fiber = GetNutrient(nutrients, 1079),
                ServingSize = "100g",
                Source = "USDA API"
            };
        }
        catch (Exception)
        {
            return await _fallback.SearchByNameAsync(foodName, cancellationToken);
        }
    }

    public Task<FoodItem?> RecognizeFromPhotoAsync(Stream photoStream, CancellationToken cancellationToken = default)
    {
        // Photo recognition requires a vision API key; delegate to mock OCR simulation.
        return _fallback.RecognizeFromPhotoAsync(photoStream, cancellationToken);
    }

    private static double GetNutrient(JsonElement nutrients, int nutrientId)
    {
        foreach (var nutrient in nutrients.EnumerateArray())
        {
            if (nutrient.TryGetProperty("nutrientId", out var id) && id.GetInt32() == nutrientId)
            {
                if (nutrient.TryGetProperty("value", out var value))
                {
                    return value.GetDouble();
                }
            }
        }

        return 0;
    }
}
