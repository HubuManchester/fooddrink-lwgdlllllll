using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FoodNutritionApp.ViewModels;

public partial class LocationViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _latitudeText = "—";

    [ObservableProperty]
    private string _longitudeText = "—";

    [ObservableProperty]
    private string _addressText = "Address not loaded yet.";

    [ObservableProperty]
    private string _accuracyText = string.Empty;

    public LocationViewModel()
    {
        Title = "Location";
        StatusMessage = "Tap the button to read GPS coordinates from this device.";
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = "Requesting location permission and GPS fix...";

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                StatusMessage = "Location permission was denied. Enable it in device settings.";
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location == null)
            {
                StatusMessage = "Could not obtain a location fix. Try again on a device or emulator with location enabled.";
                return;
            }

            LatitudeText = location.Latitude.ToString("F6");
            LongitudeText = location.Longitude.ToString("F6");
            AccuracyText = location.Accuracy.HasValue
                ? $"Accuracy: {location.Accuracy:F1} metres"
                : string.Empty;

            await ResolveAddressAsync(location);
            StatusMessage = "Location updated successfully.";
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Geolocation is not supported on this device.";
        }
        catch (PermissionException)
        {
            StatusMessage = "Location permission was denied.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to read location: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResolveAddressAsync(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            if (placemark == null)
            {
                AddressText = "Address unavailable (coordinates only).";
                return;
            }

            var parts = new[]
            {
                placemark.Thoroughfare,
                placemark.Locality,
                placemark.AdminArea,
                placemark.CountryName
            }.Where(p => !string.IsNullOrWhiteSpace(p));

            AddressText = string.Join(", ", parts);
            if (string.IsNullOrWhiteSpace(AddressText))
            {
                AddressText = "Address unavailable (coordinates only).";
            }
        }
        catch
        {
            AddressText = "Reverse geocoding failed. Latitude and longitude are still available.";
        }
    }
}
