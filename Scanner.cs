#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/Scanner.cs
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

namespace NumeralSynthesizer;

public sealed class Scanner
{
    private static readonly IReadOnlyDictionary<string, Token> KeywordTokens;

    private SourceText _sourceText;

    static Scanner()
    {
        var keywordTokens = new List<Token>
        {
            SyntaxFactory.TokenUn(),
            SyntaxFactory.TokenEt(),
            SyntaxFactory.TokenDeux(),
            SyntaxFactory.TokenTrois(),
            SyntaxFactory.TokenQuatre(),
            SyntaxFactory.TokenCinq(),
            SyntaxFactory.TokenSix(),
            SyntaxFactory.TokenSept(),
            SyntaxFactory.TokenHuit(),
            SyntaxFactory.TokenNeuf(),
            SyntaxFactory.TokenDix(),
            SyntaxFactory.TokenOnze(),
            SyntaxFactory.TokenDouze(),
            SyntaxFactory.TokenTreize(),
            SyntaxFactory.TokenQuatorze(),
            SyntaxFactory.TokenQuinze(),
            SyntaxFactory.TokenSeize(),
            SyntaxFactory.TokenDixSept(),
            SyntaxFactory.TokenDixHuit(),
            SyntaxFactory.TokenDixNeuf(),
            SyntaxFactory.TokenVingt(),
            SyntaxFactory.TokenVingts(),
            SyntaxFactory.TokenTrente(),
            SyntaxFactory.TokenQuarante(),
            SyntaxFactory.TokenCinquante(),
            SyntaxFactory.TokenSoixante(),
            SyntaxFactory.TokenQuatreVingt(),
            SyntaxFactory.TokenQuatreVingts(),
            SyntaxFactory.TokenCent(),
            SyntaxFactory.TokenCents(),
            SyntaxFactory.TokenMille(),
            SyntaxFactory.TokenMillion(),
            SyntaxFactory.TokenMillions(),
            SyntaxFactory.TokenMilliard(),
            SyntaxFactory.TokenMilliards()
        };
        KeywordTokens = keywordTokens.ToDictionary(tk => tk.Content, FunctionHelper.Identity);
    }

    public Scanner(SourceText sourceText)
    {
        _sourceText = sourceText;
    }

    public TokenStream AllTokens()
    {
        var list = new List<Token>();
        while (NextToken() is { } token)
        {
            list.Add(token);
        }

        return new TokenStream(Transform(list.ToArray()));
    }

    // 狠狠的炫技
    private IEnumerable<Token> Transform(Token[] input)
    {
        var list = new List<Token>(input);
        var indexes = new PriorityQueue<int, int>(Comparer<int>.Create((x, y) => -x.CompareTo(y)));
        for (var i = 0; i < input.Length;)
        {
            if (i + 2 <= input.Length - 1 &&
                input[i..(i + 3)] is 
                    [(_, TokenKind.Quatre), (_, TokenKind.Slash), (_, TokenKind.Vingts or TokenKind.Vingt)] or
                    [(_, TokenKind.Dix), (_, TokenKind.Slash), (_, TokenKind.Sept or TokenKind.Huit or TokenKind.Neuf)])
            {
                indexes.Enqueue(i, i);
                i += 3;
                continue;
            }

            i++;
        }

        // ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        while (indexes.TryDequeue(out var index, out _))
        {
            switch (list[index].Kind)
            {
                case TokenKind.Dix:
                    var ahead = list[index + 2];
                    list.RemoveRange(index, 3);
                    list.Insert(index, ahead.Kind switch
                    {
                        TokenKind.Sept => SyntaxFactory.TokenDixSept(),
                        TokenKind.Huit => SyntaxFactory.TokenDixHuit(),
                        TokenKind.Neuf => SyntaxFactory.TokenDixNeuf(),
                        _ => throw new ArgumentOutOfRangeException()
                    });
                    break;
                case TokenKind.Quatre:
                    var isVingts = list[index + 2] is (_, TokenKind.Vingts);
                    list.RemoveRange(index, 3);
                    list.Insert(index, isVingts ? SyntaxFactory.TokenQuatreVingts() : SyntaxFactory.TokenQuatreVingt());
                    break;
                default:
                    continue;
            }
            
        }

        return list;
    } 

    public Token? NextToken()
    {
        switch (_sourceText.Pioneer())
        {
            
            case Some<char>(var ch ) when char.IsLetter(ch):
                return ScanKeyword();
            case Some<char>('-'):
                _sourceText.One();
                return SyntaxFactory.TokenSlash();
            case Some<char>(' '):
                _sourceText.AdvanceWhile(char.IsWhiteSpace);
                _sourceText.CatchUp();
                // ReSharper disable once TailRecursiveCall
                return NextToken();
            case None<char>:
                return null;
            default:
                throw new ParseException($"Illegal character {_sourceText.Pioneer().Get()}");
        }
    }

    private Token ScanKeyword()
    {
        _sourceText.AdvanceWhile(char.IsLetter);
        var text = _sourceText.WindowAndCatchUp().Get();
        if (KeywordTokens.TryGetValue(text.Content, out var token))
        {
            return token;
        }

        throw new ParseException($"Unknown keyword at {text.Range.Start}: {text.Content}");
    }
}