namespace FoodNutritionApp.Services;

/// <summary>
/// Ensures microphone permission for speech-to-text on Android and other platforms.
/// </summary>
public static class SpeechPermissionHelper
{
    public static async Task<bool> EnsureMicrophoneAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.Android)
        {
            // User previously denied — RequestAsync may not show dialog again.
            status = await Permissions.RequestAsync<Permissions.Microphone>();
        }
        else if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
        }

        return status == PermissionStatus.Granted;
    }
}
