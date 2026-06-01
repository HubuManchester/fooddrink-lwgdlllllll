using System.Text.Json;
using FoodNutritionApp.Models;

namespace FoodNutritionApp.Services;

/// <summary>
/// Loads bundled local JSON food data — queried before any remote mock API.
/// </summary>
public class LocalFoodDataService
{
    private List<LocalFoodEntry>? _entries;

    private async Task EnsureLoadedAsync()
    {
        if (_entries != null)
        {
            return;
        }

        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("foods.json");
            _entries = await JsonSerializer.DeserializeAsync<List<LocalFoodEntry>>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            _entries = [];
        }
    }

    public async Task<FoodItem?> SearchByNameAsync(string foodName, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync();
        if (string.IsNullOrWhiteSpace(foodName) || _entries == null || _entries.Count == 0)
        {
            return null;
        }

        var query = foodName.Trim();

        var exact = _entries.FirstOrDefault(e =>
            e.Name.Equals(query, StringComparison.OrdinalIgnoreCase) ||
            e.Keywords.Any(k => k.Equals(query, StringComparison.OrdinalIgnoreCase)));

        if (exact != null)
        {
            return exact.ToFoodItem("Local");
        }

        var partial = _entries.FirstOrDefault(e =>
            e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            e.Keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase)));

        return partial?.ToFoodItem("Local");
    }

    public async Task<FoodItem?> RecognizeFromPhotoAsync(Stream photoStream, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync();
        if (_entries == null || _entries.Count == 0)
        {
            return null;
        }

        if (photoStream.CanSeek)
        {
            photoStream.Seek(0, SeekOrigin.Begin);
        }

        var buffer = new byte[4096];
        var read = await photoStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
        var hash = 0;
        for (var i = 0; i < read; i++)
        {
            hash = (hash * 31) + buffer[i];
        }

        if (photoStream.CanSeek)
        {
            photoStream.Seek(0, SeekOrigin.Begin);
        }

        var index = Math.Abs(hash) % _entries.Count;
        return _entries[index].ToFoodItem("Local Camera");
    }

    public async Task<IReadOnlyList<FoodItem>> GetAllFoodsAsync()
    {
        await EnsureLoadedAsync();
        return _entries?.Select(e => e.ToFoodItem("Local")).ToList() ?? [];
    }

    private sealed class LocalFoodEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Other";
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbohydrates { get; set; }
        public double Fiber { get; set; }
        public string ServingSize { get; set; } = "100g";
        public List<string> Keywords { get; set; } = [];

        public FoodItem ToFoodItem(string source)
        {
            return new FoodItem
            {
                Name = Name,
                Category = Category,
                Calories = Calories,
                Protein = Protein,
                Fat = Fat,
                Carbohydrates = Carbohydrates,
                Fiber = Fiber,
                ServingSize = ServingSize,
                Source = source
            };
        }
    }
}
