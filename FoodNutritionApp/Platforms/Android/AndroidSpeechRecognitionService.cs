using Android.Content;
using Android.OS;
using Android.Speech;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.AppCompat.App;
using FoodNutritionApp.Services;
using AndroidSpeechRecognizer = Android.Speech.SpeechRecognizer;
using AndroidRecognitionListener = Android.Speech.IRecognitionListener;
using AndroidBundle = Android.OS.Bundle;

namespace FoodNutritionApp.Platforms.Android;

/// <summary>
/// Uses the system speech UI (RecognizerIntent) first, then in-app SpeechRecognizer — more reliable on real devices.
/// </summary>
public sealed class AndroidSpeechRecognitionService : ISpeechRecognitionService
{
    private static readonly string[] LanguageTags = ["en-US", "en-GB", "en", "zh-CN"];

    private readonly object _lock = new();
    private ActivityResultLauncher? _launcher;
    private TaskCompletionSource<string?>? _intentTcs;

    public async Task<string?> RecognizeFoodNameAsync(CancellationToken cancellationToken = default)
    {
        var context = Microsoft.Maui.ApplicationModel.Platform.AppContext
                      ?? global::Android.App.Application.Context;

        if (context == null)
        {
            throw new InvalidOperationException("Android context is not available.");
        }

        if (!AndroidSpeechRecognizer.IsRecognitionAvailable(context))
        {
            throw new FeatureNotSupportedException(
                "Speech recognition is not available. Install Google app and Speech Services by Google.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var language in LanguageTags)
        {
            var fromIntent = await TryRecognizerIntentAsync(language, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(fromIntent))
            {
                return fromIntent.Trim();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        foreach (var language in LanguageTags)
        {
            var fromRecognizer = await TrySpeechRecognizerAsync(language, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(fromRecognizer))
            {
                return fromRecognizer.Trim();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        return null;
    }

    private Task<string?> TryRecognizerIntentAsync(string languageTag, CancellationToken cancellationToken)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity as AppCompatActivity;
            if (activity == null)
            {
                return null;
            }

            var intent = BuildRecognizerIntent(languageTag);
            if (intent.ResolveActivity(activity.PackageManager) == null)
            {
                return null;
            }

            var launcher = EnsureLauncher(activity);
            _intentTcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                launcher.Launch(intent);

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(60));
                return await _intentTcs.Task.WaitAsync(timeout.Token).ConfigureAwait(true);
            }
            catch (System.OperationCanceledException)
            {
                return null;
            }
            finally
            {
                _intentTcs = null;
            }
        });
    }

    private ActivityResultLauncher EnsureLauncher(AppCompatActivity activity)
    {
        lock (_lock)
        {
            if (_launcher != null)
            {
                return _launcher;
            }

            _launcher = activity.RegisterForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                new ActivityResultCallback(OnIntentResult));

            return _launcher;
        }
    }

    private void OnIntentResult(ActivityResult result)
    {
        var tcs = _intentTcs;
        if (tcs == null)
        {
            return;
        }

        if (result.ResultCode != (int)global::Android.App.Result.Ok || result.Data == null)
        {
            tcs.TrySetResult(null);
            return;
        }

        var matches = result.Data.GetStringArrayListExtra(AndroidSpeechRecognizer.ResultsRecognition);
        var best = matches?
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

        tcs.TrySetResult(best);
    }

    private static Intent BuildRecognizerIntent(string languageTag)
    {
        var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        intent.PutExtra(RecognizerIntent.ExtraLanguage, languageTag);
        intent.PutExtra(RecognizerIntent.ExtraPrompt, "Say a food name, e.g. apple or rice");
        intent.PutExtra(RecognizerIntent.ExtraMaxResults, 5);
        intent.PutExtra(RecognizerIntent.ExtraPartialResults, false);
        return intent;
    }

    private static Task<string?> TrySpeechRecognizerAsync(string languageTag, CancellationToken cancellationToken)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var context = Microsoft.Maui.ApplicationModel.Platform.AppContext
                          ?? global::Android.App.Application.Context;

            if (context == null)
            {
                return null;
            }

            var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var recognizer = AndroidSpeechRecognizer.CreateSpeechRecognizer(context);
            var listener = new SpeechListener(tcs, recognizer);

            recognizer.SetRecognitionListener(listener);

            try
            {
                var listenIntent = BuildRecognizerIntent(languageTag);
                recognizer.StartListening(listenIntent);

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(20));
                return await tcs.Task.WaitAsync(timeout.Token).ConfigureAwait(true);
            }
            catch (System.OperationCanceledException)
            {
                return null;
            }
            finally
            {
                try
                {
                    recognizer.StopListening();
                    recognizer.Cancel();
                    recognizer.Destroy();
                }
                catch
                {
                    // Ignore.
                }
            }
        });
    }

    private sealed class ActivityResultCallback : Java.Lang.Object, IActivityResultCallback
    {
        private readonly Action<ActivityResult> _onResult;

        public ActivityResultCallback(Action<ActivityResult> onResult)
        {
            _onResult = onResult;
        }

        public void OnActivityResult(Java.Lang.Object? result)
        {
            if (result is ActivityResult activityResult)
            {
                _onResult(activityResult);
            }
        }
    }

    private sealed class SpeechListener : Java.Lang.Object, AndroidRecognitionListener
    {
        private readonly TaskCompletionSource<string?> _tcs;
        private readonly AndroidSpeechRecognizer _recognizer;
        private bool _completed;

        public SpeechListener(TaskCompletionSource<string?> tcs, AndroidSpeechRecognizer recognizer)
        {
            _tcs = tcs;
            _recognizer = recognizer;
        }

        public void OnResults(AndroidBundle? results)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            var matches = results?.GetStringArrayList(AndroidSpeechRecognizer.ResultsRecognition);
            _tcs.TrySetResult(matches?.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)));
        }

        public void OnError(SpeechRecognizerError error)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _tcs.TrySetResult(null);
        }

        public void OnReadyForSpeech(AndroidBundle? @params)
        {
        }

        public void OnBeginningOfSpeech()
        {
        }

        public void OnRmsChanged(float rmsdB)
        {
        }

        public void OnBufferReceived(byte[]? buffer)
        {
        }

        public void OnEndOfSpeech()
        {
        }

        public void OnPartialResults(AndroidBundle? partialResults)
        {
        }

        public void OnEvent(int eventType, AndroidBundle? @params)
        {
        }
    }
}
