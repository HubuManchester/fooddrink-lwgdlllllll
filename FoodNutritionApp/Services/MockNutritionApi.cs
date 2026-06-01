using FoodNutritionApp.Models;

namespace FoodNutritionApp.Services;

/// <summary>
/// Mock nutrition API with predefined foods for development and emulator testing.
/// Simulates OCR/vision by mapping image hash to a food name.
/// </summary>
public class MockNutritionApi : INutritionApi
{
    private static readonly Dictionary<string, FoodItem> FoodDatabase = new(StringComparer.OrdinalIgnoreCase)
    {
        ["apple"] = new() { Name = "Apple", Category = "Fruit", Calories = 52, Protein = 0.3, Fat = 0.2, Carbohydrates = 14, Fiber = 2.4, ServingSize = "100g" },
        ["banana"] = new() { Name = "Banana", Category = "Fruit", Calories = 89, Protein = 1.1, Fat = 0.3, Carbohydrates = 23, Fiber = 2.6, ServingSize = "100g" },
        ["orange"] = new() { Name = "Orange", Category = "Fruit", Calories = 47, Protein = 0.9, Fat = 0.1, Carbohydrates = 12, Fiber = 2.4, ServingSize = "100g" },
        ["chicken"] = new() { Name = "Chicken Breast", Category = "Meat", Calories = 165, Protein = 31, Fat = 3.6, Carbohydrates = 0, Fiber = 0, ServingSize = "100g" },
        ["rice"] = new() { Name = "White Rice", Category = "Grain", Calories = 130, Protein = 2.7, Fat = 0.3, Carbohydrates = 28, Fiber = 0.4, ServingSize = "100g" },
        ["bread"] = new() { Name = "Whole Wheat Bread", Category = "Grain", Calories = 247, Protein = 13, Fat = 3.4, Carbohydrates = 41, Fiber = 7, ServingSize = "100g" },
        ["egg"] = new() { Name = "Boiled Egg", Category = "Dairy", Calories = 155, Protein = 13, Fat = 11, Carbohydrates = 1.1, Fiber = 0, ServingSize = "100g" },
        ["milk"] = new() { Name = "Whole Milk", Category = "Dairy", Calories = 61, Protein = 3.2, Fat = 3.3, Carbohydrates = 4.8, Fiber = 0, ServingSize = "100ml" },
        ["salmon"] = new() { Name = "Salmon", Category = "Meat", Calories = 208, Protein = 20, Fat = 13, Carbohydrates = 0, Fiber = 0, ServingSize = "100g" },
        ["broccoli"] = new() { Name = "Broccoli", Category = "Vegetable", Calories = 34, Protein = 2.8, Fat = 0.4, Carbohydrates = 7, Fiber = 2.6, ServingSize = "100g" },
        ["pizza"] = new() { Name = "Cheese Pizza", Category = "Other", Calories = 266, Protein = 11, Fat = 10, Carbohydrates = 33, Fiber = 2.3, ServingSize = "100g" },
        ["burger"] = new() { Name = "Beef Burger", Category = "Other", Calories = 295, Protein = 17, Fat = 14, Carbohydrates = 24, Fiber = 1.5, ServingSize = "100g" },
        ["苹果"] = new() { Name = "Apple (苹果)", Category = "Fruit", Calories = 52, Protein = 0.3, Fat = 0.2, Carbohydrates = 14, Fiber = 2.4, ServingSize = "100g" },
        ["香蕉"] = new() { Name = "Banana (香蕉)", Category = "Fruit", Calories = 89, Protein = 1.1, Fat = 0.3, Carbohydrates = 23, Fiber = 2.6, ServingSize = "100g" },
    };

    private static readonly string[] RecognizedFoods = ["Apple", "Banana", "Orange", "Chicken Breast", "Broccoli", "Salmon"];

    public async Task<FoodItem?> SearchByNameAsync(string foodName, CancellationToken cancellationToken = default)
    {
        await Task.Delay(600, cancellationToken);

        if (string.IsNullOrWhiteSpace(foodName))
        {
            return null;
        }

        var trimmed = foodName.Trim();

        if (FoodDatabase.TryGetValue(trimmed, out var exact))
        {
            return CloneWithSource(exact, "Search");
        }

        var match = FoodDatabase.Values.FirstOrDefault(f =>
            f.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            return CloneWithSource(match, "Search");
        }

        return null;
    }

    public async Task<FoodItem?> RecognizeFromPhotoAsync(Stream photoStream, CancellationToken cancellationToken = default)
    {
        // Simulate network + OCR processing delay
        await Task.Delay(1200, cancellationToken);

        var hash = await ComputeStreamHashAsync(photoStream, cancellationToken);
        var index = Math.Abs(hash) % RecognizedFoods.Length;
        var recognizedName = RecognizedFoods[index];

        var item = FoodDatabase.Values.First(f => f.Name == recognizedName);
        return CloneWithSource(item, "Camera");
    }

    private static FoodItem CloneWithSource(FoodItem source, string sourceType)
    {
        return new FoodItem
        {
            Name = source.Name,
            Category = source.Category,
            Calories = source.Calories,
            Protein = source.Protein,
            Fat = source.Fat,
            Carbohydrates = source.Carbohydrates,
            Fiber = source.Fiber,
            ServingSize = source.ServingSize,
            Source = sourceType
        };
    }

    private static async Task<int> ComputeStreamHashAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var buffer = new byte[4096];
        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
        var hash = 0;

        for (var i = 0; i < read; i++)
        {
            hash = (hash * 31) + buffer[i];
        }

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        return hash;
    }
}
