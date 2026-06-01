namespace FoodNutritionApp.Services;

/// <summary>
/// Default light sensor implementation for platforms without a dedicated sensor API.
/// Returns a moderate light level so flash auto-enable can be demonstrated manually.
/// </summary>
public class DefaultLightSensorService : ILightSensorService
{
    public bool IsSupported => false;

    public Task<float> GetLightLevelAsync(CancellationToken cancellationToken = default)
    {
        // Emulator-friendly default: treat as low light to demonstrate flashlight logic in code.
        return Task.FromResult(15f);
    }
}
