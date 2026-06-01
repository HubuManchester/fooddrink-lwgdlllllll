namespace FoodNutritionApp.Models;

/// <summary>
/// Represents nutritional information for a food item returned from the API.
/// </summary>
public class FoodItem
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Other";
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbohydrates { get; set; }
    public double Fiber { get; set; }
    public string ServingSize { get; set; } = "100g";
    public string Source { get; set; } = "Search";

    /// <summary>
    /// Builds a spoken summary for text-to-speech accessibility.
    /// </summary>
    public string ToSpeechSummary()
    {
        return $"{Name}, category {Category}. Per {ServingSize}: {Calories:F0} kilocalories. " +
               $"Protein {Protein:F1} grams. Fat {Fat:F1} grams. " +
               $"Carbohydrates {Carbohydrates:F1} grams. Fiber {Fiber:F1} grams.";
    }
}
