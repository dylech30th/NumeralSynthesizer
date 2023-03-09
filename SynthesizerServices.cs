using System.Diagnostics.CodeAnalysis;
using Microsoft.CognitiveServices.Speech;
using Sharprompt;

namespace NumeralSynthesizer;

[SuppressMessage("Usage", "CA2211:Les champs non constants ne doivent pas être visibles")]
public static class SynthesizerServices
{
    public static Task NumberPractice(SpeechSynthesizer synthesizer, long minInclusive, long maxExclusive, PracticeConfig config)
    {
        var type = Prompt.Select<PracticeOption>("Please choose the mode you want to practice");
        return type switch
        {
            PracticeOption.Practice => NumberPracticeMode(synthesizer, minInclusive, maxExclusive),
            PracticeOption.QuickReaction => NumberQuickReactionPracticeMode(synthesizer, minInclusive, maxExclusive, config.QuickReactionTimeLimit),
            PracticeOption.TimeChallenge => NumberTimeChallengePracticeMode(synthesizer, minInclusive, maxExclusive, config.TimeChallengeTimeLimitInMinutes),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static Task NumeralPractice(long minInclusive, long maxExclusive, PracticeConfig config)
    {
        var type = Prompt.Select<PracticeOption>("Please choose the mode you want to practice");
        switch (type)
        {
            case PracticeOption.Practice:
                NumeralPracticeMode(minInclusive, maxExclusive, config.StrictParsing);
                return Task.CompletedTask;
            case PracticeOption.QuickReaction:
                return NumeralQuickReactionMode(minInclusive, maxExclusive, config.QuickReactionTimeLimit, config.StrictParsing);
            case PracticeOption.TimeChallenge:
                return NumeralTimeChallengePracticeMode(minInclusive, maxExclusive, config.TimeChallengeTimeLimitInMinutes, config.StrictParsing);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static async Task NumeralTimeChallengePracticeMode(long minInclusive, long maxExclusive, int timeLimit,
        bool stringMode)
    {
        var counter = new PracticeCounter(stringMode ? NumeralComparer.Strict : NumeralComparer.NonStrict);
        var tcs = new TaskCompletionSource();

        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(timeLimit));
            tcs.SetResult();
        });
        _ = Task.Run(() =>
        {
            do
            {
                if (tcs.Task.IsCompleted) break;
                var next = Random.Shared.NextLong(minInclusive, maxExclusive).ToString();
                ConsoleHelper.WriteLineWithColor(ConsoleColor.Yellow, $"{next}?");
                var version = counter.Ask(Numeraliser.Parse(next));
                var read = Console.ReadLine()!.Trim();
                if (tcs.Task.IsCompleted) break;
                counter.Answer(version, read);
            } while (counter.LastState is not PracticeCounter.AnswerState.Interrupted);
        });

        await tcs.Task;

        if (Environment.OSVersion.Platform is PlatformID.Win32NT)
        {
            var handle = NativeInterops.GetStdHandle(NativeInterops.StdInputHandle);
            NativeInterops.CancelIoEx(handle, nint.Zero);
        }

        Congrats(counter);
    }

    private static async Task NumeralQuickReactionMode(long minInclusive, long maxExclusive, int timeLimit, bool strictMode)
    {
        var counter = new PracticeCounter(strictMode ? NumeralComparer.Strict : NumeralComparer.NonStrict);
        var inputBuffer = new InputBuffer();
        inputBuffer.Start();
        do
        {
            var tcs = new TaskCompletionSource();

            var next = Random.Shared.NextLong(minInclusive, maxExclusive).ToString();
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Yellow, $"{next}?");
            
            var version = counter.Ask(Numeraliser.Parse(next));
            inputBuffer.ReadBarrierRelease();

            _ = Task.Run(async () =>
            {
                await Task.Delay(timeLimit * 1000);
                counter.TimeoutIfNotAnswered(version);
                tcs.TrySetResult();
            });
            _ = Task.Run(async () =>
            {
                await inputBuffer.WaitToReadAsync();
                if (counter.Version == version && inputBuffer.CanRead)
                {
                    counter.Answer(version, await inputBuffer.ReadAsync());
                    tcs.TrySetResult();
                }
            });
            await tcs.Task;
            inputBuffer.ReadBarrier();
        } while (counter.LastState is not PracticeCounter.AnswerState.Interrupted);

        inputBuffer.Stop();
        Congrats(counter, true);
    }
    
    private static void NumeralPracticeMode(long minInclusive, long maxExclusive, bool strictMode)
    {
        var counter = new PracticeCounter(strictMode ? NumeralComparer.Strict : NumeralComparer.NonStrict);
        do
        {
            var next = Random.Shared.NextLong(minInclusive, maxExclusive).ToString();
            int version;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{next}?");
            Console.ForegroundColor = ConsoleColor.White;
            do
            {
                version = counter.Ask(Numeraliser.Parse(next));
            } while (counter.Answer(version, Console.ReadLine()!.Trim()) is PracticeCounter.AnswerState.Wrong);
        } while (counter.LastState is not PracticeCounter.AnswerState.Interrupted);
        
        Congrats(counter);
    }

    private static async Task NumberTimeChallengePracticeMode(SpeechSynthesizer synthesizer, long minInclusive, long maxExclusive, int timeLimit)
    {
        var counter = new PracticeCounter();
        var tcs = new TaskCompletionSource();

        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(timeLimit));
            tcs.SetResult();
        });
        _ = Task.Run(async () =>
        {
            do
            {
                if (tcs.Task.IsCompleted) break;
                var next = Random.Shared.NextLong(minInclusive, maxExclusive);
                var result = await synthesizer.SpeakTextAsync(next.ToString());
                HandleSynthesisFailure(result);
                var version = counter.Ask(next);
                var read = Console.ReadLine()!.Trim();
                if (tcs.Task.IsCompleted) break;
                counter.Answer(version, read);
            } while (counter.LastState is not PracticeCounter.AnswerState.Interrupted);
        });

        await tcs.Task;
        
        if (Environment.OSVersion.Platform is PlatformID.Win32NT)
        {
            var handle = NativeInterops.GetStdHandle(NativeInterops.StdInputHandle);
            NativeInterops.CancelIoEx(handle, nint.Zero);
        }
        
        Congrats(counter);
    }

    private static async Task NumberQuickReactionPracticeMode(SpeechSynthesizer synthesizer, long minInclusive, long maxExclusive, int timeLimit)
    {
        var counter = new PracticeCounter();
        var inputBuffer = new InputBuffer();
        inputBuffer.Start();
        do
        {
            var tcs = new TaskCompletionSource();

            var next = Random.Shared.NextLong(minInclusive, maxExclusive);
            var result = await synthesizer.SpeakTextAsync(next.ToString());
            HandleSynthesisFailure(result);
            
            var version = counter.Ask(next);
            
            inputBuffer.ReadBarrierRelease();

            _ = Task.Run(async () =>
            {
                await Task.Delay(timeLimit * 1000);
                counter.TimeoutIfNotAnswered(version);
                tcs.TrySetResult();
            });
            _ = Task.Run(async () =>
            {
                await inputBuffer.WaitToReadAsync();
                if (counter.Version == version && inputBuffer.CanRead)
                {
                    counter.Answer(version, await inputBuffer.ReadAsync());
                    tcs.TrySetResult();
                }
            });
            await tcs.Task;
            inputBuffer.ReadBarrier();
        } while (counter.LastState is not PracticeCounter.AnswerState.Interrupted);

        inputBuffer.Stop();
        Congrats(counter, true);
    }

    private static async Task NumberPracticeMode(SpeechSynthesizer synthesizer, long minInclusive, long maxExclusive)
    {
        var counter = new PracticeCounter();
        do
        {
            var next = Random.Shared.NextLong(minInclusive, maxExclusive);
            int version;
            do
            {
                var result = await synthesizer.SpeakTextAsync(next.ToString());
                HandleSynthesisFailure(result);
                version = counter.Ask(next);
            } while (counter.Answer(version, Console.ReadLine()!.Trim()) is PracticeCounter.AnswerState.Wrong);
        } while (counter.LastState is not PracticeCounter.AnswerState.Interrupted);

        Congrats(counter);
    }

    private static void HandleSynthesisFailure(SpeechSynthesisResult result)
    {
        if (result.Reason == ResultReason.Canceled)
        {
            var detail = SpeechSynthesisCancellationDetails.FromResult(result);
            if (detail.Reason == CancellationReason.Error)
            {
                ConsoleHelper.WithColor(ConsoleColor.Red, () =>
                {
                    Console.WriteLine("Failed to synthesis voice for the following reason:");
                    Console.WriteLine($"Code {detail.ErrorCode}: {detail.ErrorDetails}");
                });
                Environment.Exit(-1);
            }
        }
    }
    
    private static void Congrats(PracticeCounter counter, bool withTimeouts = false)
    {
        var total = counter.Correct + counter.Wrong + counter.Timeouts;
        ConsoleHelper.WriteLineWithColor(ConsoleColor.Green, $"You have answered {counter.Correct} questions correctly out of {total} questions in {counter.TotalSeconds:F} seconds. Correct rate: {counter.Correct / (double) total:P}");

        if (withTimeouts)
        {
            ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, $"You have {counter.Timeouts} questions that didn't get answered within time limit. ");
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        ConsoleHelper.WithColor(ConsoleColor.Cyan, () =>
        {
            Console.WriteLine($"Your average response time is {counter.TotalSeconds / (counter.Correct + counter.Wrong):F} seconds. ");
            Console.WriteLine($"Your average correct response time is {counter.TotalSeconds / counter.Correct:F} seconds. ");
        });
    }
}