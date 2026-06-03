using System.Globalization;

namespace FoodNutritionApp.Services;

/// <summary>
/// Text-to-speech via MAUI (Android / iOS). Runs on main thread for device compatibility.
/// </summary>
public sealed class MauiTextToSpeechPlaybackService : ISpeechPlaybackService
{
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;

    public bool IsSpeaking { get; private set; }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("No text to speak.");
        }

        CancellationToken token;
        lock (_lock)
        {
            CancelLocked();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            token = _cts.Token;
        }

        IsSpeaking = true;

        try
        {
            token.ThrowIfCancellationRequested();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // First try with best locale; if that fails, speak with system default.
                try
                {
                    var options = await BuildSpeechOptionsAsync();
                    await TextToSpeech.Default.SpeakAsync(text, options, token);
                }
                catch (Exception) when (!token.IsCancellationRequested)
                {
                    await TextToSpeech.Default.SpeakAsync(text, cancelToken: token);
                }
            });
        }
        catch (OperationCanceledException)
        {
            // User tapped Stop.
        }
        finally
        {
            lock (_lock)
            {
                _cts?.Dispose();
                _cts = null;
            }

            IsSpeaking = false;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            CancelLocked();
        }

        IsSpeaking = false;
    }

    private static async Task<SpeechOptions> BuildSpeechOptionsAsync()
    {
        var options = new SpeechOptions
        {
            Pitch = 1.0f,
            Volume = 1.0f
        };

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            if (locales == null || !locales.Any())
            {
                return options;
            }

            var locale =
                locales.FirstOrDefault(l => l.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
                ?? locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                ?? locales.FirstOrDefault(l =>
                    l.Language.StartsWith(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                        StringComparison.OrdinalIgnoreCase))
                ?? locales.FirstOrDefault();

            if (locale != null)
            {
                options.Locale = locale;
            }
        }
        catch
        {
            // Use default engine voice.
        }

        return options;
    }

    private void CancelLocked()
    {
        if (_cts == null)
        {
            return;
        }

        try
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore.
        }
    }
}
