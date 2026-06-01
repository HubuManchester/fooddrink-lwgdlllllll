using Android.Content;
using Android.Hardware;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.Platforms.Android;

/// <summary>
/// Android ambient light sensor used to trigger flashlight in dark environments.
/// </summary>
public class AndroidLightSensorService : ILightSensorService
{
    private const float LowLightThresholdLux = 30f;
    private readonly SensorManager? _sensorManager;
    private readonly Sensor? _lightSensor;

    public AndroidLightSensorService()
    {
        var context = Platform.AppContext;
        _sensorManager = context.GetSystemService(Context.SensorService) as SensorManager;
        _lightSensor = _sensorManager?.GetDefaultSensor(SensorType.Light);
    }

    public bool IsSupported => _lightSensor != null;

    public Task<float> GetLightLevelAsync(CancellationToken cancellationToken = default)
    {
        if (_sensorManager == null || _lightSensor == null)
        {
            return Task.FromResult(15f);
        }

        var tcs = new TaskCompletionSource<float>();

        var listener = new LightSensorListener(value =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetResult(value);
            }
        });

        _sensorManager.RegisterListener(listener, _lightSensor, SensorDelay.Normal);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(800, cancellationToken);
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetResult(LowLightThresholdLux - 1);
                }
            }
            catch (TaskCanceledException)
            {
                tcs.TrySetCanceled(cancellationToken);
            }
            finally
            {
                _sensorManager.UnregisterListener(listener);
            }
        }, cancellationToken);

        return tcs.Task;
    }

    private sealed class LightSensorListener : Java.Lang.Object, ISensorEventListener
    {
        private readonly Action<float> _onChanged;

        public LightSensorListener(Action<float> onChanged)
        {
            _onChanged = onChanged;
        }

        public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent? e)
        {
            if (e?.Values == null || e.Values.Count == 0)
            {
                return;
            }

            _onChanged(e.Values[0]);
        }
    }
}
