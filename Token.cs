#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/Token.cs
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

using System.Reflection;

namespace NumeralSynthesizer;

[AttributeUsage(AttributeTargets.Field)]
public class NumeralAttribute : Attribute
{
    public int Numeral { get; }

    public NumeralAttribute(int numeral)
    {
        Numeral = numeral;
    }
}

public static class NumeralAttributeHelper
{
    public static int? GetNumeral<T>(this T e) where T : Enum =>
        typeof(T).GetMember(e.ToString())[0].GetCustomAttribute<NumeralAttribute>()?.Numeral;

    public static int GetNumeralUnsafe<T>(this T e) where T : Enum =>
        e.GetNumeral() ?? throw new ParseException($"Expecting a enum member that is attached by [Numeral] attribute but {e} was found.");
}

public enum TokenKind
{
    Slash,
    Et,

    [Numeral(1)]
    Un,

    [Numeral(2)]
    Deux,

    [Numeral(3)]
    Trois,

    [Numeral(4)]
    Quatre,

    [Numeral(5)]
    Cinq,

    [Numeral(6)]
    Six,

    [Numeral(7)]
    Sept,

    [Numeral(8)]
    Huit,

    [Numeral(9)]
    Neuf,

    [Numeral(10)]
    Dix,

    [Numeral(11)]
    Onze,

    [Numeral(12)]
    Douze,

    [Numeral(13)]
    Treize,

    [Numeral(14)]
    Quatorze,

    [Numeral(15)]
    Quinze,

    [Numeral(16)]
    Seize,

    [Numeral(17)]
    DixSept,

    [Numeral(18)]
    DixHuit,

    [Numeral(19)]
    DixNeuf,

    [Numeral(20)]
    Vingt,

    [Numeral(20)]
    Vingts, // Vingts is a transient token

    [Numeral(30)]
    Trente,

    [Numeral(40)]
    Quarante,

    [Numeral(50)]
    Cinquante,

    [Numeral(60)]
    Soixante,

    [Numeral(80)]
    QuatreVingt,

    [Numeral(80)]
    QuatreVingts,

    [Numeral(100)]
    Cent,
    
    [Numeral(100)]
    Cents,

    [Numeral(1000)]
    Mille,

    [Numeral(1000000)]
    Million,

    [Numeral(1000000)]
    Millions,

    [Numeral(1000000000)]
    Milliard,

    [Numeral(1000000000)]
    Milliards,
}

public record Token(string Content, TokenKind Kind);

public static class SyntaxFactory
{
    public static Token TokenSlash() => new("-", TokenKind.Slash);

    public static Token TokenEt() => new("et", TokenKind.Et);

    public static Token TokenUn() => new("un", TokenKind.Un);

    public static Token TokenDeux() => new("deux", TokenKind.Deux);

    public static Token TokenTrois() => new("trois", TokenKind.Trois);

    public static Token TokenQuatre() => new("quatre", TokenKind.Quatre);

    public static Token TokenCinq() => new("cinq", TokenKind.Cinq);

    public static Token TokenSix() => new("six", TokenKind.Six);

    public static Token TokenSept() => new("sept", TokenKind.Sept);

    public static Token TokenHuit() => new("huit", TokenKind.Huit);

    public static Token TokenNeuf() => new("neuf", TokenKind.Neuf);

    public static Token TokenDix() => new("dix", TokenKind.Dix);

    public static Token TokenOnze() => new("onze", TokenKind.Onze);

    public static Token TokenDouze() => new("douze", TokenKind.Douze);

    public static Token TokenTreize() => new("treize", TokenKind.Treize);

    public static Token TokenQuatorze() => new("quatorze", TokenKind.Quatorze);

    public static Token TokenQuinze() => new("quinze", TokenKind.Quinze);

    public static Token TokenSeize() => new("seize", TokenKind.Seize);

    public static Token TokenDixSept() => new("dix-sept", TokenKind.DixSept);

    public static Token TokenDixHuit() => new("dix-huit", TokenKind.DixHuit);

    public static Token TokenDixNeuf() => new("dix-neuf", TokenKind.DixNeuf);

    public static Token TokenVingt() => new("vingt", TokenKind.Vingt);

    public static Token TokenVingts() => new("vingts", TokenKind.Vingts);

    public static Token TokenTrente() => new("trente", TokenKind.Trente);

    public static Token TokenQuarante() => new("quarante", TokenKind.Quarante);

    public static Token TokenCinquante() => new("cinquante", TokenKind.Cinquante);

    public static Token TokenSoixante() => new("soixante", TokenKind.Soixante);

    public static Token TokenQuatreVingt() => new("quatre-vingt", TokenKind.QuatreVingt);

    public static Token TokenQuatreVingts() => new("quatre-vingts", TokenKind.QuatreVingts);

    public static Token TokenCent() => new("cent", TokenKind.Cent);
    
    public static Token TokenCents() => new("cents", TokenKind.Cents);

    public static Token TokenMille() => new("mille", TokenKind.Mille);

    public static Token TokenMillion() => new("million", TokenKind.Million);

    public static Token TokenMillions() => new("millions", TokenKind.Millions);

    public static Token TokenMilliard() => new("milliard", TokenKind.Milliard);

    public static Token TokenMilliards() => new("milliards", TokenKind.Milliards);
}