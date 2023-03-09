#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/TokenStream.cs
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

public interface ITokenStream
{
    Token[] All();

    Token Dequeue();

    Token[] Dequeue(int count);

    bool TryPeek(out Token? token);

    Token Peek();

    bool TryPeek(int count, out Token[] token);

    Token[] Peek(int count);
}

public class TokenStream : ITokenStream
{
    private readonly Queue<Token> _tokens;

    public TokenStream()
    {
        _tokens = new Queue<Token>();
    }

    public TokenStream(IEnumerable<Token> tokens)
    {
        _tokens = new Queue<Token>(tokens);
    }

    public void Enqueue(Token token)
    {
        _tokens.Enqueue(token);
    }

    public Token[] All()
    {
        return _tokens.ToArray();
    }

    public Token Dequeue()
    {
        return _tokens.Dequeue();
    }

    public Token[] Dequeue(int count)
    {
        if (_tokens.Count >= count)
        {
            var results = _tokens.Take(count).ToArray();
            var counter = count;
            do _tokens.Dequeue(); while (counter-- > 0);
            return results;
        }

        throw new InvalidOperationException("Not enough tokens to be dequeued");
    }

    public bool TryPeek(out Token? token)
    {
        return _tokens.TryPeek(out token);
    }

    public Token Peek()
    {
        if (TryPeek(out var token))
        {
            return token!;
        }

        throw new InvalidOperationException("Expecting a token but EOF was found");
    }

    public bool TryPeek(int count, out Token[] token)
    {
        if (_tokens.Count < count)
        {
            token = Array.Empty<Token>();
            return false;
        }
        token = _tokens.Take(count).ToArray();
        return true;
    }

    public Token[] Peek(int count)
    {
        if (TryPeek(count, out var token))
        {
            return token!;
        }

        throw new InvalidOperationException($"Expecting at least {count} token(s) but EOF was found");
    }
}