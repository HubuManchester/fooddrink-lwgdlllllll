using System.Net.Http.Json;
using System.Text.Json;
using FoodNutritionApp.Models;

namespace FoodNutritionApp.Services;

/// <summary>
/// USDA FoodData Central search. Returns null when offline or no match (caller uses mock fallback).
/// </summary>
public class RealNutritionApi : INutritionApi
{
    private readonly HttpClient _httpClient;

    public RealNutritionApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<FoodItem?> SearchByNameAsync(string foodName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(foodName))
        {
            return null;
        }

        try
        {
            var url =
                $"https://api.nal.usda.gov/fdc/v1/foods/search?query={Uri.EscapeDataString(foodName.Trim())}&pageSize=1&api_key=DEMO_KEY";
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("foods", out var foods) || foods.GetArrayLength() == 0)
            {
                return null;
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
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task<FoodItem?> RecognizeFromPhotoAsync(Stream photoStream, CancellationToken cancellationToken = default)
    {
        // USDA has no photo OCR endpoint.
        return Task.FromResult<FoodItem?>(null);
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
