using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Kurukuru;
using Sharprompt;

namespace NumeralSynthesizer;

[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
[SuppressMessage("Usage", "CA2208:Instancier les exceptions d\'argument correctement")]
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var speechKeyOption = new Option<string?>(new[] { "-key", "--speech-key" }, "The key for the Azure Speech Service");
        var speechRegionOption = new Option<string?>(new[] { "-region", "--speech-region" }, "The region for the Azure Speech Service");
        var configPath = new Option<string?>(new[] { "-config", "--config-path" }, "The path to the configuration file");

        var rootCommand = new RootCommand
        {
            speechKeyOption,
            speechRegionOption,
            configPath
        };
        
        rootCommand.SetHandler(async (key, region, fileInfo) =>
        {
            if (fileInfo != null)
            {
                if (File.Exists(fileInfo) && new FileInfo(fileInfo) is { } fi &&
                    JsonSerializer.Deserialize<PracticeConfig>(await File.ReadAllTextAsync(fi.FullName)) is { SpeechKey.Length: > 0, SpeechRegion.Length: > 0 } config)
                {
                    await Run(config, fi.FullName);
                    return;
                }

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
                {
                    key = Prompt.Input<string>("Please enter the key for the Azure Speech Service");
                    region = Prompt.Input<string>("Please enter the region for the Azure Speech Service");
                }
                    
                await Run(new PracticeConfig(key, region), new FileInfo(fileInfo).FullName);
                return;
            }
            
            
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
            {
                key = Prompt.Input<string>("Please enter the key for the Azure Speech Service");
                region = Prompt.Input<string>("Please enter the region for the Azure Speech Service");
            }
            
            await Run(new PracticeConfig(key, region));
        }, speechKeyOption, speechRegionOption, configPath);

        await rootCommand.InvokeAsync(args);
    }

    private static async Task Run(PracticeConfig config, string configFilePath = "config.json")
    {
        var synthesizerPool = new SynthesizerPool(config);

        do
        {
            var command = Prompt.Select<CommandOption>("Choose the command you'd like to perform");
            switch (command)
            {
                case CommandOption.SubDecimal:
                    await SynthesizerServices.NumberPractice(synthesizerPool.Shared, 1, 11, config);
                    break;
                case CommandOption.SubCent:
                    await SynthesizerServices.NumberPractice(synthesizerPool.Shared, 1, 101, config);
                    break;
                case CommandOption.SubMille:
                    await SynthesizerServices.NumberPractice(synthesizerPool.Shared, 1, 1001, config);
                    break;
                case CommandOption.SubMillion:
                    await SynthesizerServices.NumberPractice(synthesizerPool.Shared, 1, 1000001, config);
                    break;
                case CommandOption.SubMilliard:
                    await SynthesizerServices.NumberPractice(synthesizerPool.Shared, 1, 1000000001, config);
                    break;
                case CommandOption.FullRandom:
                    await SynthesizerServices.NumberPractice(synthesizerPool.Shared, 1, 9999999999, config);
                    break;
                case CommandOption.WriteNumeralsSubDecimal: 
                    await SynthesizerServices.NumeralPractice(1, 11, config);
                    break;
                case CommandOption.WriteNumeralsSubCent:
                    await SynthesizerServices.NumeralPractice(1, 101, config);
                    break;
                case CommandOption.WriteNumeralsSubMille:
                    await SynthesizerServices.NumeralPractice(1, 1001, config);
                    break;
                case CommandOption.WriteNumeralsSubMillion:
                    await SynthesizerServices.NumeralPractice(1, 1000001, config);
                    break;
                case CommandOption.WriteNumeralsSubMilliard:
                    await SynthesizerServices.NumeralPractice(1, 1000000001, config);
                    break;
                case CommandOption.WriteNumeralsFullRandom:
                    await SynthesizerServices.NumeralPractice(1, 9999999999, config);
                    break;
                case CommandOption.ChangeVoices:
                    var task = synthesizerPool.GetVoicesAsync("fr-FR");
                    if (!task.IsCompleted)
                    {
                        await Spinner.StartAsync("Retrieving available voices...", async spinner =>
                        {
                            await task;
                            spinner.Text = "Retrieve completed!";
                        });
                    }

                    switch (await task)
                    {
                        case (_, { Length: > 0 } str):
                            await Console.Error.WriteLineAsync($"Error in retrieving voice actors: {str}");
                            break;
                        case var (voices, _):
                            var select = Prompt.Select($"Select the voice you want to apply. Current value: {config.VoiceActor}", voices, textSelector: info => $"{info.Name} ({info.ShortName})");
                            synthesizerPool.Modify(p => p with { VoiceActor = select.ShortName });
                            config = config with { VoiceActor = select.ShortName };
                            ConsoleHelper.WriteLineWithColor(ConsoleColor.Green, $"✔️ Successfully set the voice actor to {select.ShortName}");
                            break;
                    }
                    break;
                case CommandOption.ChangeQuickReactionTime:
                    var qrTime = Prompt.Input<int>($"Enter the time in seconds (1~20) for the quick reaction test. Current value: {config.QuickReactionTimeLimit}'s");
                    if (qrTime is < 1 or > 20)
                    {
                        ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, "❌ The time must be between 1 and 20 seconds");
                        return;
                    }

                    config = config with { QuickReactionTimeLimit = qrTime };
                    ConsoleHelper.WriteLineWithColor(ConsoleColor.Green, $"Successfully set quick reaction time limit to {qrTime}");
                    break;
                case CommandOption.ChangeTimeChallengeTime:
                    var challengeTime = Prompt.Input<int>($"Enter the time in minutes (1-10) for the time challenge test. Current value: {config.QuickReactionTimeLimit}'m");
                    if (challengeTime is < 1 or > 10)
                    {
                        ConsoleHelper.WriteLineWithColor(ConsoleColor.Red, "❌ The time must be between 1 and 10 minutes");
                        return;
                    }

                    config = config with { TimeChallengeTimeLimitInMinutes = challengeTime };
                    ConsoleHelper.WriteLineWithColor(ConsoleColor.Green, $"Successfully set time challenge time limit to {challengeTime}");
                    break;
                case CommandOption.ParsingDecision:
                    var parsingDecision = Prompt.Select<ParsingDecision>($"Select the parsing decision you want to set. Current value: {(config.StrictParsing ? "strict parsing" : "non-strict parsing")}");
                    config = config with { StrictParsing = parsingDecision == ParsingDecision.Strict };
                    ConsoleHelper.WriteLineWithColor(ConsoleColor.Green, $"Successfully set your parsing decision to {(config.StrictParsing ? "strict parsing" : "non-strict parsing")}");
                    break;
                case CommandOption.Exit:
                    await File.WriteAllTextAsync(configFilePath, JsonSerializer.Serialize(config));
                    return;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown option: {command}");
            }
        } while (true);
    }
}