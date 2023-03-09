#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/SourceText.cs
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
using static NumeralSynthesizer.Options;

namespace NumeralSynthesizer;

public record LineInfo(int LineNumber, int ColumnNumber)
{
    public override string ToString() => $"({LineNumber}:{ColumnNumber})";
}

public record TextWindow(string Content, Range Range, (LineInfo, LineInfo) RangeLineInfo)
{
    public static readonly TextWindow Dummy = new("", ..0, (new LineInfo(0, 0), new LineInfo(0, 0)));
}

[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public record struct SourceText(string Text)
{
    public const char Eof = '\u0000';

    private int _index;
    private int _forward;

    public void Reset()
    {
        _index = 0;
        _forward = 0;
    }

    public IOption<char> Current() => _index < Text.Length ? Some(Text[_index]) : None<char>();

    public IOption<char> Pioneer() => _forward < Text.Length ? Some(Text[_forward]) : None<char>();

    public IOption<char> Peek(int n = 1) => _forward + n < Text.Length ? Some(Text[_forward + n]) : None<char>();

    public void Advance(int n = 1) => _forward += n;

    public IOption<char> Next()
    {
        Advance();
        return Pioneer();
    }

    public void CatchUp() => _index = _forward;

    public readonly IOption<TextWindow> Window()
    {
        return _forward <= Text.Length
            ? Some(new TextWindow(Text[_index.._forward], _index.._forward, (PositionOf(_index), PositionOf(_forward))))
            : None<TextWindow>();
    }

    public IOption<TextWindow> WindowAndCatchUp()
    {
        var window = Window();
        CatchUp();
        return window;
    }

    public void Return(int n = 1) => _forward -= n;

    public IOption<TextWindow> One()
    {
        Advance();
        return WindowAndCatchUp();
    }

    public IOption<char> GetAndAdvance(int n = 1)
    {
        var result = Pioneer();
        Advance(n);
        return result;
    }

    public IOption<char> AdvanceAndGet(int n = 1)
    {
        Advance(n);
        return Pioneer();
    }

    public readonly LineInfo Position() => PositionOf(_forward);

    public void AdvanceWhile(Func<char, bool> predicate)
    {
        switch (Pioneer())
        {
            case Some<char>(var c) when predicate(c):
                Advance();
                // ReSharper disable once TailRecursiveCall
                AdvanceWhile(predicate);
                break;
        }
    }

    private readonly LineInfo PositionOf(int index)
    {
        var slice = Text[..Math.Min(index, Text.Length)];
        var counter = 1 + slice.Count(c => c == '\n');
        return new LineInfo(counter, index - slice.LastIndexOf('\n'));
    }

    // implicit operator
    public static implicit operator SourceText(string text) => new(text);
}