using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NumeralSynthesizer;

public record PracticeConfig(string SpeechKey, string SpeechRegion, int QuickReactionTimeLimit = 2, int TimeChallengeTimeLimitInMinutes = 1, string VoiceActor = "fr-FR-AlainNeural", bool StrictParsing = true);

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public sealed class PracticeCounter
{
    public enum AnswerState
    {
        Correct,
        Wrong,
        Interrupted,
        Timeout
    }

    private string _correctAnswer;

    private readonly IEqualityComparer<string> _equalityComparer;
    
    private readonly Stopwatch _stopwatch;
    
    public int Version { get; private set; }
    
    public int Correct { get; private set; }

    public int Wrong { get; private set; }

    public int Timeouts { get; private set; }

    public bool LastQuestionAnswered { get; private set; }

    public double TotalSeconds { get; private set; }

    public AnswerState LastState { get; private set; }

    public PracticeCounter(IEqualityComparer<string>? equalityComparer = null)
    {
        _correctAnswer = "";
        _stopwatch = new Stopwatch();
        _equalityComparer = equalityComparer ?? EqualityComparer<string>.Default;
    }

    public int Ask(long number)
    {
        return Ask(number.ToString());
    }
    
    public int Ask(string number)
    {
        _correctAnswer = number;
        _stopwatch.Restart();
        LastQuestionAnswered = false;
        return ++Version;
    }

    public void TimeoutIfNotAnswered(int version)
    {
        if (!LastQuestionAnswered)
        {
            Timeout(version);
        }
    }

    public void Timeout(int version)
    {
        if (version != Version)
        {
            return;
        }

        Timeouts++;
        ConsoleHelper.WriteLineWithColor(ConsoleColor.Magenta, $"⌚ Timeout! The correct answer is {_correctAnswer}. Try better next time!");
        TotalSeconds += _stopwatch.ElapsedMilliseconds / 1000d;
        _stopwatch.Stop();
        LastState = AnswerState.Timeout;
    }

    public AnswerState Answer(int version, string text)
    {
        if (version != Version)
        {
            return LastState;
        }
        
        LastQuestionAnswered = true;
        switch (text)
        {
            case "exit":
                _stopwatch.Stop();
                LastState = AnswerState.Interrupted;
                return LastState;
            case var _ when _equalityComparer.Equals(text, _correctAnswer):
                Correct++;
                ConsoleHelper.WriteLineWithColor(ConsoleColor.Green, "✔️ Correct answer!");
                TotalSeconds += _stopwatch.ElapsedMilliseconds / 1000d;
                _stopwatch.Stop();
                LastState = AnswerState.Correct;
                return LastState;
            default:
                Wrong++;
                ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, $"❌ Wrong answer! Correct answer is {_correctAnswer}. Try better next time!");
                TotalSeconds += _stopwatch.ElapsedMilliseconds / 1000d;
                _stopwatch.Stop();
                LastState = AnswerState.Wrong;
                return LastState;
        }
    }
}