namespace FoodNutritionApp.Services;

/// <summary>
/// Reads ambient light level from the device sensor when available.
/// </summary>
public interface ILightSensorService
{
    Task<float> GetLightLevelAsync(CancellationToken cancellationToken = default);
    bool IsSupported { get; }
}
