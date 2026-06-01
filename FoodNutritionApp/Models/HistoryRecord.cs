using SQLite;

namespace FoodNutritionApp.Models;

/// <summary>
/// SQLite entity for saved scan and search history.
/// </summary>
[Table("HistoryRecords")]
public class HistoryRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string FoodName { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbohydrates { get; set; }
    public double Fiber { get; set; }
    public string ServingSize { get; set; } = "100g";
    public string Source { get; set; } = "Search";
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public static HistoryRecord FromFoodItem(FoodItem item)
    {
        return new HistoryRecord
        {
            FoodName = item.Name,
            Calories = item.Calories,
            Protein = item.Protein,
            Fat = item.Fat,
            Carbohydrates = item.Carbohydrates,
            Fiber = item.Fiber,
            ServingSize = item.ServingSize,
            Source = item.Source,
            SavedAt = DateTime.UtcNow
        };
    }

    public FoodItem ToFoodItem()
    {
        return new FoodItem
        {
            Name = FoodName,
            Calories = Calories,
            Protein = Protein,
            Fat = Fat,
            Carbohydrates = Carbohydrates,
            Fiber = Fiber,
            ServingSize = ServingSize,
            Source = Source
        };
    }
}
