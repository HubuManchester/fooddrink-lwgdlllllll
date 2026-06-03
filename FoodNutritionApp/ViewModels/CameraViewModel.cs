using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodNutritionApp.Models;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.ViewModels;

public partial class CameraViewModel : BaseViewModel
{
    private const float LowLightThresholdLux = 30f;

    private readonly INutritionApi _nutritionApi;
    private readonly DatabaseService _databaseService;
    private readonly ILightSensorService _lightSensorService;

    [ObservableProperty]
    private ImageSource? _capturedImage;

    [ObservableProperty]
    private string _lightLevelText = "Checking ambient light...";

    public CameraViewModel(
        INutritionApi nutritionApi,
        DatabaseService databaseService,
        ILightSensorService lightSensorService)
    {
        _nutritionApi = nutritionApi;
        _databaseService = databaseService;
        _lightSensorService = lightSensorService;
        Title = "Photo Recognition";
    }

    [RelayCommand]
    private async Task CaptureAndRecognizeAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;
        var flashWasTurnedOn = false;

        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                StatusMessage = "Camera is not available on this device. Please use manual search instead.";
                return;
            }

            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (cameraStatus != PermissionStatus.Granted)
            {
                StatusMessage = "Camera permission was denied. Open Settings → Apps → FoodNutritionApp → Permissions → allow Camera.";
                return;
            }

            var lightLevel = await _lightSensorService.GetLightLevelAsync();
            LightLevelText = $"Ambient light: {lightLevel:F0} lux";

            if (lightLevel < LowLightThresholdLux)
            {
                try
                {
                    await Flashlight.Default.TurnOnAsync();
                    flashWasTurnedOn = true;
                    StatusMessage = "Low light detected. Flash enabled for better photo quality.";
                }
                catch (FeatureNotSupportedException)
                {
                    StatusMessage = "Low light detected. Flash is not available on this device.";
                }
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Capture food photo"
            });

            if (photo == null)
            {
                StatusMessage = "Photo capture was cancelled.";
                return;
            }

            CapturedImage = ImageSource.FromFile(photo.FullPath);

            await using var stream = await photo.OpenReadAsync();
            var foodItem = await _nutritionApi.RecognizeFromPhotoAsync(stream);

            if (foodItem == null)
            {
                StatusMessage = "Could not recognize food from this photo. Please try again or use manual search.";
                return;
            }

            if (Vibration.Default.IsSupported)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            }

            await _databaseService.SaveRecordAsync(foodItem);

            var navigationParameter = new ShellNavigationQueryParameters
            {
                { "FoodItem", foodItem }
            };

            await Shell.Current.GoToAsync(nameof(Views.DetailPage), navigationParameter);
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Camera is not supported on this device.";
        }
        catch (PermissionException)
        {
            StatusMessage = "Camera permission was denied. Please enable it in device settings.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Something went wrong while capturing the photo: {ex.Message}";
        }
        finally
        {
            if (flashWasTurnedOn)
            {
                try
                {
                    await Flashlight.Default.TurnOffAsync();
                }
                catch
                {
                    // Flash may not be available on emulators; ignore cleanup errors.
                }
            }

            IsBusy = false;
        }
    }
}
