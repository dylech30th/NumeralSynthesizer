#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/Parser.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace NumeralSynthesizer;

public static class Numeraliser
{
    private static readonly string[] SubVingt = { "", "onze", "douze", "treize", "quatorze", "quinze", "seize", "dix-sept", "dix-huit", "dix-neuf" };
    
    private static readonly string[] Units = { "", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf" };
    
    private static readonly string[] Tens = { "", "dix", "vingt", "trente", "quarante", "cinquante", "soixante", "soixante-dix", "quatre-vingts", "quatre-vingt-dix" };

    [SuppressMessage("ReSharper", "TailRecursiveCall")]
    public static string Parse(string number)
    {
        if (number.Length >= 10)
        {
            var upper = number[..^9];
            var lower = number[^9..];
            return upper.Length > 0
                ? upper == "un"
                    ? "un milliard " + Parse(lower)
                    : Parse(upper) + " milliards " + Parse(lower)
                : Parse(lower);
        }

        return (number.Length switch
        {
            < 4 => ParseSubMille(int.Parse(number)),
            < 7 => ParseSubMillion(int.Parse(number)),
            < 10 => ParseSubMilliard(int.Parse(number)),
            _ => throw new FormatException($"{number} is not a valid number format")
        }).Trim();
    }
    
    private static string ParseSubMilliard(int number)
    {
        var subMillion = ParseSubMillion(number % 1000000);
        var upper = ParseSubMillion(number / 1000000);
        upper = upper.EndsWith("cents") ? upper.TrimEnd('s') : upper;
        var plural = upper is not "un";
        return $"{upper} {(plural ? "millions" : "million")} {subMillion}";
    }

    private static string ParseSubMillion(int number)
    {
        var subMille = ParseSubMille(number % 1000);
        var upper = ParseSubMille(number / 1000);
        return upper.Length == 0 ? subMille : $"{(upper.EndsWith("cents") ? upper.TrimEnd('s') : upper)} mille {subMille}";
    }

    private static string ParseSubMille(int number)
    {
        var unite = number % 10;
        var dix = number / 10 % 10;
        var cent = number / 100;

        var subCent = (dix, unite) switch
        {
            (0, 0) => string.Empty,
            (0, _) => Units[unite],
            (>= 1 and <= 9, 0) => Tens[dix],
            (1, >= 1 and <= 9) => SubVingt[unite],
            (>= 2 and <= 6, 1) => $"{Tens[dix]} et un",
            (>= 2 and <= 6, _) => $"{Tens[dix]}-{Units[unite]}",
            (7, 1) => "soixante et onze",
            (7, _) => $"soixante-{SubVingt[unite]}",
            (8, 1) => "quatre-vingt-un",
            (8, _) => $"quatre-vingt-{Units[unite]}",
            (9, 1) => $"quatre-vingt-onze",
            (9, _) => $"quatre-vingt-{SubVingt[unite]}",
            _ => throw new FormatException($"Illegal number format {number}")
        };

        var c = cent switch
        {
            0 => "",
            1 => "cent",
            _ => subCent.Length == 0 ? $"{Units[cent]} cents" : $"{Units[cent]} cent"
        };

        return (c, subCent) switch
        {
            ({ Length: 0 }, { Length: 0 }) => string.Empty,
            ({ Length: 0 }, _) => subCent,
            (_, { Length: 0 }) => c,
            _ => $"{c} {subCent}"
        };
    }
}

public sealed class Parser
{
    private readonly ITokenStream _tokenStream;

    public Parser(ITokenStream tokenStream)
    {
        _tokenStream = tokenStream;
    }

    public BigInteger Parse()
    {
        if (_tokenStream.All() is { } list && list.Any(t => t.Kind is TokenKind.Milliard or TokenKind.Milliards))
        {
            var lastIndex = list.LastIndexOf(t => t.Kind is TokenKind.Milliards or TokenKind.Milliard);

            if (lastIndex == 0)
            {
                throw new ParseException(@"""milliard"" or ""milliards"" must be prefixed with proper numerals");
            }

            var multiplier = new Parser(new TokenStream(list[..lastIndex])).Parse();

            if (multiplier == 1 && list[lastIndex].Kind is TokenKind.Milliards)
            {
                throw new ParseException($@"Use plural form ""milliards"" after a plural numeral ""{multiplier}""");
            }

            if (multiplier != 1 && list[lastIndex].Kind is TokenKind.Milliard)
            {
                throw new ParseException($@"Use singular form ""milliard"" after a singular numeral ""{multiplier}""");
            }

            var rest = new Parser(new TokenStream(list[(lastIndex + 1)..])).Parse();
            return multiplier * 1000000000 + rest;
        }

        if (_tokenStream.TryPeek(out _))
        {
            var firstMultiplier = ParseCent();
            if (ParseMillion() is { } millionToken)
            {
                return (millionToken.Kind, firstMultiplier) switch
                {
                    (TokenKind.Million, not 1) => throw new ParseException(@"Use plural form ""millions"" after a plural numeral"),
                    (TokenKind.Millions, 1) => throw new ParseException(@"Use singular form ""million"" after a singular numeral"),
                    (_, { } multiplier) => multiplier * 1000000 + Parse(),
                    _ => throw new ParseException(@"""million"" must be used together with proper numeral prefixes")
                };
            }
            
            if (ParseMille() is not null)
            {
                return firstMultiplier is { } milleMultiplier
                    ? milleMultiplier * 1000 + Parse()
                    : 1000 + Parse();
            }

            return firstMultiplier ?? 0;
        }

        return 0;
    }

    private Token? ParseMille()
    {
        if (_tokenStream.TryPeek(out var token) && token!.Kind is TokenKind.Mille)
        {
            return EatToken();
        }

        return null;
    }

    private Token? ParseMillion()
    {
        if (_tokenStream.TryPeek(out var token) && token!.Kind is TokenKind.Million or TokenKind.Millions)
        {
            return EatToken();
        }

        return null;
    }

    private int? ParseCent()
    {
        if (_tokenStream.TryPeek(out var token))
        {
            switch (token)
            {
                case { Kind: TokenKind.Cent }:
                    EatToken();
                    return 100 + ParseSubCent();
                case var _ when IsSubCentStart(token!):
                    var multiplier = ParseSubCent();
                    if (_tokenStream.TryPeek(out var t) && t!.Kind is TokenKind.Cent or TokenKind.Cents)
                    {
                        EatToken();
                        if (t.Kind is TokenKind.Cents && _tokenStream.TryPeek(out _))
                        {
                            throw new ParseException(@"""Cents"" can only appear at the very end of the string.");
                        }
                        return _tokenStream.TryPeek(out var subsequent) && IsSubCentStart(subsequent!)
                            ? multiplier >= 10
                                ? throw new ParseException("Multiplier of 100 cannot exceeds 9")
                                : multiplier * 100 + ParseSubCent()
                            : multiplier * 100;
                    }

                    return multiplier;
                default:
                    return null;
            }
        }

        return null;
    }

    private int ParseSubCent()
    {
        if (_tokenStream.TryPeek(out var token))
        {
            switch (token!.Kind)
            {
                case TokenKind.Dix:
                    EatToken();
                    return 10;
                case TokenKind.Vingt:
                    EatToken();
                    return 20 + ParseDecimalSuffix();
                case TokenKind.Trente:
                    EatToken();
                    return 30 + ParseDecimalSuffix();
                case TokenKind.Quarante:
                    EatToken();
                    return 40 + ParseDecimalSuffix();
                case TokenKind.Cinquante:
                    EatToken();
                    return 50 + ParseDecimalSuffix();
                case TokenKind.Soixante:
                    EatToken();
                    return ParseSoixante();
                case TokenKind.QuatreVingt:
                    EatToken();
                    return ParseQuatreVingt();
                case TokenKind.QuatreVingts:
                    EatToken();
                    return 80;
                case var _ when IsDecimal(token):
                    return ParseDecimal().Kind.GetNumeralUnsafe();
                case var _ when IsSubVingt(token):
                    return ParseSubVingt().Kind.GetNumeralUnsafe();
                default:
                    return 0;
            }
        }

        return 0;
    }

    private int ParseQuatreVingt()
    {
        EatToken(TokenKind.Slash);
        var peek = _tokenStream.Peek();
        switch (peek.Kind)
        {
            case TokenKind.Dix:
                EatToken();
                return 90;
            case var _ when IsDecimal(peek):
                return 80 + ParseDecimal().Kind.GetNumeralUnsafe();
            default:
                return 80 + ParseSubVingt().Kind.GetNumeralUnsafe();
        }
    }

    private int ParseSoixante()
    {
        if (_tokenStream.TryPeek(out var token))
        {
            switch (token!.Kind)
            {
                case TokenKind.Et:
                    EatToken();
                    var numeral = EatToken(TokenKind.Un, TokenKind.Onze).Kind.GetNumeralUnsafe();
                    return 60 + numeral;
                case TokenKind.Slash:
                    EatToken();
                    var peek = _tokenStream.Peek();

                    if (peek.Kind == TokenKind.Dix)
                    {
                        EatToken();
                        return 70;
                    }
                    return IsDecimalSansUn(peek)
                        ? 60 + ParseDecimalSansUn().Kind.GetNumeralUnsafe()
                        : 60 + ParseSubVingtSansOnze().Kind.GetNumeralUnsafe();

                default:
                    return 60;
            }
        }

        return 60;
    }

    private Token ParseDecimal()
    {
        return _tokenStream.Peek().Kind == TokenKind.Un ? EatToken() : ParseDecimalSansUn();
    }

    private Token ParseDecimalSansUn()
    {
        return EatToken(TokenKind.Deux, TokenKind.Trois, TokenKind.Quatre, TokenKind.Cinq, TokenKind.Six, TokenKind.Sept, TokenKind.Huit, TokenKind.Neuf);
    }

    private Token ParseSubVingt()
    {
        return _tokenStream.Peek().Kind == TokenKind.Onze ? EatToken() : ParseSubVingtSansOnze();
    }

    private Token ParseSubVingtSansOnze()
    {
        return EatToken(TokenKind.Douze, TokenKind.Treize, TokenKind.Quatorze, TokenKind.Quinze, TokenKind.Seize, TokenKind.DixSept, TokenKind.DixHuit, TokenKind.DixNeuf);
    }

    private static bool IsDecimal(Token token)
    {
        return token.Kind == TokenKind.Un || IsDecimalSansUn(token);
    }

    private static bool IsDecimalSansUn(Token token)
    {
        return token.Kind is TokenKind.Deux or TokenKind.Trois or TokenKind.Quatre or TokenKind.Cinq or TokenKind.Six or TokenKind.Sept or TokenKind.Huit or TokenKind.Neuf;
    }

    private static bool IsSubVingt(Token token)
    {
        return token.Kind is TokenKind.Onze || IsSubVingtSansOnze(token);
    }

    private static bool IsSubVingtSansOnze(Token token)
    {
        return token.Kind is TokenKind.Douze or TokenKind.Treize or TokenKind.Quatorze or TokenKind.Quinze or TokenKind.Seize or TokenKind.DixSept or TokenKind.DixHuit or TokenKind.DixNeuf;
    }

    private static bool IsSubCentStart(Token token)
    {
        return IsDecimal(token) || IsSubVingt(token) || token.Kind is TokenKind.Dix or TokenKind.Vingt or TokenKind.Trente or TokenKind.Quarante or TokenKind.Cinquante or TokenKind.Soixante or TokenKind.QuatreVingt or TokenKind.QuatreVingts;
    }

    private int ParseDecimalSuffix()
    {
        if (_tokenStream.TryPeek(out var token) && token!.Kind is TokenKind.Slash or TokenKind.Et)
        {
            EatToken();
            switch (token.Kind)
            {
                case TokenKind.Et:
                    EatToken(TokenKind.Un);
                    return 1;
                default:
                    return EatToken().Kind.GetNumeralUnsafe();
            }
        }

        return 0;
    }

    private Token EatToken(TokenKind? expectation = null)
    {
        if (_tokenStream.TryPeek(out var result) && result is { })
        {
            return expectation is null
                ? _tokenStream.Dequeue()
                : result.Kind == expectation
                    ? _tokenStream.Dequeue()
                    : throw new ParseException($"Expecting a token of kind {expectation} but {result.Kind} was found");
        }

        throw new ParseException($"Expecting a token of kind {expectation} but EOF was found");
    }

    private Token EatToken(params TokenKind[] expectation)
    {
        if (_tokenStream.TryPeek(out var result) && result is { })
        {
            return expectation.Length == 0
                ? _tokenStream.Dequeue()
                : expectation.Any(e => e == result.Kind)
                    ? _tokenStream.Dequeue()
                    : throw new ParseException($"Expecting a token of kind {string.Join(" | ", expectation)} but {result.Kind} was found");
        }

        throw new ParseException($"Expecting a token of kind {expectation} but EOF was found");
    }
}