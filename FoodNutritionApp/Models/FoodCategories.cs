namespace FoodNutritionApp.Models;

/// <summary>
/// Food category constants used for filtering and classification.
/// </summary>
public static class FoodCategories
{
    public const string All = "All";

    public static readonly string[] FilterOptions =
    [
        All, "Fruit", "Vegetable", "Meat", "Dairy", "Grain", "Drink", "Other"
    ];

    public static readonly string[] EditableOptions =
    [
        "Fruit", "Vegetable", "Meat", "Dairy", "Grain", "Drink", "Other"
    ];
}
