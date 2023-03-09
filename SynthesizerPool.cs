using Microsoft.CognitiveServices.Speech;

namespace NumeralSynthesizer;

public sealed class SynthesizerPool
{
    private PracticeConfig _config;
    private SpeechSynthesizer? _pooledInstance;
    private SpeechConfig? _speechConfig;
    private VoiceInfo[]? _availableVoices;

    public SpeechSynthesizer Shared => GetShared();

    public SynthesizerPool(PracticeConfig config)
    {
        _config = config;   
    }

    public void Modify(Func<PracticeConfig, PracticeConfig> synthesizerAction)
    {
        _config = synthesizerAction(_config);
        _pooledInstance = CreateSynthesizer();
    }

    public async Task<(VoiceInfo[]? voices, string? errorDetail)> GetVoicesAsync(string locale)
    {
        if (_availableVoices is not null)
        {
            return (_availableVoices, null);
        }

        var result = await Shared.GetVoicesAsync(locale);
        if (!string.IsNullOrWhiteSpace(result.ErrorDetails))
        {
            return (null, result.ErrorDetails);
        }

        _availableVoices ??= result.Voices.ToArray();
        return (_availableVoices, null);
    }

    private SpeechSynthesizer GetShared()
    {
        return _pooledInstance ??= CreateSynthesizer();
    }
    
    private SpeechSynthesizer CreateSynthesizer()
    {
        _speechConfig = SpeechConfig.FromSubscription(_config.SpeechKey, _config.SpeechRegion);
        _speechConfig.SpeechSynthesisVoiceName = _config.VoiceActor;
        return new SpeechSynthesizer(_speechConfig);
    }
}