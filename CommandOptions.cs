using System.ComponentModel.DataAnnotations;

namespace NumeralSynthesizer;

public enum ParsingDecision
{
    [Display(Name = "Strict parsing")]
    Strict,
    
    [Display(Name = "Non-strict parsing")]
    NonStrict,
}

public enum PracticeOption
{
    [Display(Name = "Practice Mode: No time limitation")]
    Practice,
    
    [Display(Name = "Quick Reaction Test: You must answer each question within the time limit")]
    QuickReaction,
    
    [Display(Name = "Time Challenge: See how many questions you can answer within a time limit")]
    TimeChallenge
}

public enum CommandOption
{
    [Display(Name = "Change the voice actor")]
    ChangeVoices,

    [Display(Name = "Set the parsing algorithm to strict (do not ignore slashes) or non-strict (ignore slashes)")]
    ParsingDecision,
    
    [Display(Name = "Change the time limit in quick reaction test")]
    ChangeQuickReactionTime,
    
    [Display(Name = "Change the time limit in time challenge test")]
    ChangeTimeChallengeTime,

    [Display(Name = "Practice with numbers below 10")]
    SubDecimal,
    
    [Display(Name = "Practice with numbers below 100")]
    SubCent,
    
    [Display(Name = "Practice with numbers below 1000")]
    SubMille,
    
    [Display(Name = "Practice with numbers below 1000000")]
    SubMillion,

    [Display(Name = "Practice with numbers below 1000000000")]
    SubMilliard,
    
    [Display(Name = "Practice with arbitrary numbers below 9999999999")]
    FullRandom,
    
    [Display(Name = "Write out numerals in French below 10")]
    WriteNumeralsSubDecimal,

    [Display(Name = "Write out numerals in French below 100")]
    WriteNumeralsSubCent,
    
    [Display(Name = "Write out numerals in French below 1000")]
    WriteNumeralsSubMille,
    
    [Display(Name = "Write out numerals in French below 1000000")]
    WriteNumeralsSubMillion,
    
    [Display(Name = "Write out numerals in French below 1000000000")]
    WriteNumeralsSubMilliard,
    
    [Display(Name = "Write out numerals in French below 9999999999")]
    WriteNumeralsFullRandom,

    [Display(Name = "exit")] 
    Exit
}