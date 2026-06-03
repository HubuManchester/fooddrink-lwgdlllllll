using Android.OS;
using Android.Speech.Tts;
using FoodNutritionApp.Services;
using Java.Util;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;
using Locale = Java.Util.Locale;
using TtsOperationResult = Android.Speech.Tts.OperationResult;

namespace FoodNutritionApp.Platforms.Android;

/// <summary>
/// Android native TTS with explicit engine init (avoids MAUI "Failed to initialize Text to Speech engine").
/// </summary>
public sealed class AndroidSpeechPlaybackService : Java.Lang.Object, ISpeechPlaybackService, AndroidTextToSpeech.IOnInitListener
{
    private static readonly string[] EnginePackages =
    [
        null!, // system default
        "com.google.android.tts",
        "com.samsung.SMT",
        "com.microsoft.cognitiveservices.speech"
    ];

    private readonly object _lock = new();
    private AndroidTextToSpeech? _tts;
    private TaskCompletionSource<bool>? _initTcs;
    private string? _activeEngine;
    private int _initAttempt;
    private bool _isReady;
    private TaskCompletionSource<bool>? _utteranceTcs;
    private CancellationTokenRegistration _cancelRegistration;

    public bool IsSpeaking { get; private set; }

    public AndroidSpeechPlaybackService()
    {
        // Warm up engine while user browses the app.
        _ = WarmUpAsync();
    }

    public async Task WarmUpAsync()
    {
        try
        {
            await EnsureReadyAsync(forceRecreate: false).ConfigureAwait(false);
        }
        catch
        {
            // Ignore — SpeakAsync will surface errors.
        }
    }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("No text to speak.");
        }

        Stop();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (!await EnsureReadyAsync(forceRecreate: false).ConfigureAwait(true))
            {
                throw new InvalidOperationException(GetUserFacingInitError());
            }

            var tts = _tts ?? throw new InvalidOperationException(GetUserFacingInitError());
            ConfigureLanguage(tts);

            var utteranceId = Guid.NewGuid().ToString("N");
            _utteranceTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            tts.SetOnUtteranceProgressListener(new UtteranceListener(_utteranceTcs, OnUtteranceFinished));

            IsSpeaking = true;

            _cancelRegistration = cancellationToken.Register(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        tts.Stop();
                    }
                    catch
                    {
                        // Ignore.
                    }

                    _utteranceTcs?.TrySetCanceled(cancellationToken);
                    IsSpeaking = false;
                });
            });

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var bundle = new Bundle();
                bundle.PutString(AndroidTextToSpeech.Engine.KeyParamUtteranceId, utteranceId);
                tts.Speak(text, QueueMode.Flush, bundle, utteranceId);
            }
            else
            {
#pragma warning disable CS0618
                tts.Speak(text, QueueMode.Flush, null);
#pragma warning restore CS0618
            }

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                await _utteranceTcs.Task.WaitAsync(timeout.Token).ConfigureAwait(true);
            }
            catch (System.OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // User tapped Stop.
            }
            finally
            {
                _cancelRegistration.Dispose();
                _cancelRegistration = default;
                IsSpeaking = false;
            }
        }).ConfigureAwait(false);
    }

    public void Stop()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                _tts?.Stop();
            }
            catch
            {
                // Ignore.
            }

            _utteranceTcs?.TrySetCanceled();
            IsSpeaking = false;
        });
    }

    public void OnInit(TtsOperationResult status)
    {
        var tcs = _initTcs;
        if (tcs == null)
        {
            return;
        }

        if (status == TtsOperationResult.Success)
        {
            lock (_lock)
            {
                _isReady = true;
            }

            tcs.TrySetResult(true);
            return;
        }

        // Try next engine package on the main thread.
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!tcs.Task.IsCompleted)
            {
                await TryStartNextEngineAsync().ConfigureAwait(true);
            }
        });
    }

    private void OnUtteranceFinished()
    {
        IsSpeaking = false;
    }

    private async Task<bool> EnsureReadyAsync(bool forceRecreate)
    {
        lock (_lock)
        {
            if (!forceRecreate && _isReady && _tts != null)
            {
                return true;
            }
        }

        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            lock (_lock)
            {
                if (!forceRecreate && _isReady && _tts != null)
                {
                    return true;
                }

                ShutdownLocked();
                _initAttempt = 0;
            }

            return await TryStartNextEngineAsync().ConfigureAwait(true);
        }).ConfigureAwait(false);
    }

    private async Task<bool> TryStartNextEngineAsync()
    {
        if (_initAttempt >= EnginePackages.Length)
        {
            lock (_lock)
            {
                _isReady = false;
            }

            _initTcs?.TrySetResult(false);
            return false;
        }

        var engine = EnginePackages[_initAttempt++];
        ShutdownLocked();

        _initTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _activeEngine = engine;

        var context = Microsoft.Maui.ApplicationModel.Platform.AppContext
                      ?? global::Android.App.Application.Context
                      ?? throw new InvalidOperationException("Android application context is not available.");

        _tts = engine == null
            ? new AndroidTextToSpeech(context, this)
            : new AndroidTextToSpeech(context, this, engine);

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(12));
            return await _initTcs.Task.WaitAsync(timeout.Token).ConfigureAwait(true);
        }
        catch
        {
            ShutdownLocked();
            return await TryStartNextEngineAsync().ConfigureAwait(true);
        }
    }

    private void ShutdownLocked()
    {
        lock (_lock)
        {
            _isReady = false;
        }

        try
        {
            _tts?.Stop();
            _tts?.Shutdown();
        }
        catch
        {
            // Ignore.
        }

        _tts = null;
        _initTcs = null;
    }

    private static void ConfigureLanguage(AndroidTextToSpeech tts)
    {
        var en = Locale.ForLanguageTag("en-US");
        var langResult = tts.SetLanguage(en);
        if (langResult < 0)
        {
            tts.SetLanguage(Locale.Default);
        }
    }

    private static string GetUserFacingInitError() =>
        "TTS engine failed. Settings → System → Text-to-speech: choose Google Text-to-speech, install English voice data, tap Listen. Then raise Media volume.";

    private sealed class UtteranceListener : UtteranceProgressListener
    {
        private readonly TaskCompletionSource<bool> _tcs;
        private readonly Action _onFinished;

        public UtteranceListener(TaskCompletionSource<bool> tcs, Action onFinished)
        {
            _tcs = tcs;
            _onFinished = onFinished;
        }

        public override void OnStart(string? utteranceId)
        {
        }

        public override void OnDone(string? utteranceId)
        {
            _onFinished();
            _tcs.TrySetResult(true);
        }

        [Obsolete("deprecated")]
        public override void OnError(string? utteranceId)
        {
            _onFinished();
            _tcs.TrySetException(new InvalidOperationException("TTS playback error."));
        }

        public override void OnError(string? utteranceId, TextToSpeechError errorCode)
        {
            _onFinished();
            _tcs.TrySetException(new InvalidOperationException($"TTS playback error: {errorCode}"));
        }
    }
}
