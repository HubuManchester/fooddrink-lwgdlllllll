namespace FoodNutritionApp.Services;

/// <summary>
/// Speech via MAUI TextToSpeech (Android / iOS / Mac). Stop uses CancellationToken only.
/// </summary>
public sealed class MauiTextToSpeechPlaybackService : ISpeechPlaybackService
{
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;

    public bool IsSpeaking { get; private set; }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
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
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            token.ThrowIfCancellationRequested();

            var locale = locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                         ?? locales.FirstOrDefault();

            var options = new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f,
                Locale = locale
            };

            await TextToSpeech.Default.SpeakAsync(text, options, token);
        }
        catch (OperationCanceledException)
        {
            // Expected when Stop() is called.
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
