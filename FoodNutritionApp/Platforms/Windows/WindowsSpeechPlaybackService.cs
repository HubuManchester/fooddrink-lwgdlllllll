using System.Speech.Synthesis;
using FoodNutritionApp.Services;

namespace FoodNutritionApp.Platforms.Windows;

/// <summary>
/// Windows TTS using one shared SpeechSynthesizer. Stop uses SpeakAsyncCancelAll() without disposing.
/// </summary>
public sealed class WindowsSpeechPlaybackService : ISpeechPlaybackService
{
    private readonly object _lock = new();
    private readonly SpeechSynthesizer _synthesizer = new();
    private TaskCompletionSource? _completionTcs;

    public bool IsSpeaking { get; private set; }

    public WindowsSpeechPlaybackService()
    {
        SelectEnglishVoice(_synthesizer);
    }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        TaskCompletionSource completionTcs;

        lock (_lock)
        {
            CancelPlaybackLocked(completeWaiter: true);
            completionTcs = _completionTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            IsSpeaking = true;
        }

        void OnSpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            lock (_lock)
            {
                if (!ReferenceEquals(_completionTcs, completionTcs))
                {
                    return;
                }
            }

            // Complete normally even when cancelled — avoids TaskCanceledException in the UI layer.
            completionTcs.TrySetResult();
        }

        _synthesizer.SpeakCompleted += OnSpeakCompleted;

        using var cancelRegistration = cancellationToken.Register(() =>
        {
            if (MainThread.IsMainThread)
            {
                Stop();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(Stop);
            }
        });

        try
        {
            _synthesizer.SpeakAsync(text);
            await completionTcs.Task.ConfigureAwait(true);
        }
        finally
        {
            _synthesizer.SpeakCompleted -= OnSpeakCompleted;
            cancelRegistration.Dispose();

            lock (_lock)
            {
                if (ReferenceEquals(_completionTcs, completionTcs))
                {
                    _completionTcs = null;
                    IsSpeaking = false;
                }
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            CancelPlaybackLocked(completeWaiter: true);
            IsSpeaking = false;
        }
    }

    private void CancelPlaybackLocked(bool completeWaiter)
    {
        try
        {
            _synthesizer.SpeakAsyncCancelAll();
        }
        catch
        {
            // Nothing playing.
        }

        if (completeWaiter)
        {
            _completionTcs?.TrySetResult();
            _completionTcs = null;
        }
    }

    private static void SelectEnglishVoice(SpeechSynthesizer synthesizer)
    {
        try
        {
            var englishVoice = synthesizer.GetInstalledVoices()
                .FirstOrDefault(v =>
                    v.Enabled &&
                    v.VoiceInfo.Culture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase));

            if (englishVoice != null)
            {
                synthesizer.SelectVoice(englishVoice.VoiceInfo.Name);
            }
        }
        catch
        {
            // Keep default voice.
        }
    }
}
