using System.Globalization;
using CommunityToolkit.Maui.Media;

namespace FoodNutritionApp.Services;

/// <summary>
/// Speech recognition via CommunityToolkit (iOS / fallback).
/// </summary>
public sealed class MauiSpeechRecognitionService : ISpeechRecognitionService
{
    private static readonly string[] CultureNames = ["en-US", "en-GB", "en"];

    public async Task<string?> RecognizeFoodNameAsync(CancellationToken cancellationToken = default)
    {
        if (!await SpeechToText.Default.RequestPermissions(cancellationToken).ConfigureAwait(false))
        {
            throw new PermissionException("Speech recognition permission was denied.");
        }

        foreach (var cultureName in CultureNames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = await ListenOnceAsync(cultureName, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Trim();
            }
        }

        return null;
    }

    private static async Task<string?> ListenOnceAsync(string cultureName, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
        {
            Detach();
            tcs.TrySetResult(e.RecognitionResult?.Text);
        }

        void Detach()
        {
            SpeechToText.Default.RecognitionResultCompleted -= OnCompleted;
        }

        SpeechToText.Default.RecognitionResultCompleted += OnCompleted;

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                CultureInfo culture;
                try
                {
                    culture = CultureInfo.GetCultureInfo(cultureName);
                }
                catch
                {
                    culture = CultureInfo.CurrentCulture;
                }

                var options = new SpeechToTextOptions { Culture = culture };
                await SpeechToText.Default.StartListenAsync(options, cancellationToken).ConfigureAwait(true);
            }).ConfigureAwait(false);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(20));

            try
            {
                return await tcs.Task.WaitAsync(timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
        finally
        {
            Detach();

            try
            {
                await SpeechToText.Default.StopListenAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // Ignore.
            }
        }
    }
}
