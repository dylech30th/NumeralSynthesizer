#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/Options.cs
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

using System.Collections;

namespace NumeralSynthesizer;

public interface IOption<out T> : IEnumerable<T>
{
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        switch (this)
        {
            case Some<T>(var t):
                yield return t;
                yield break;
            default:
                yield break;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static IOption<T> Some(T value) => new Some<T>(value);

    public static IOption<T> None => new None<T>();
}

public static class Options
{
    public static IOption<T> Some<T>(T value) => IOption<T>.Some(value);

    public static IOption<T> None<T>() => IOption<T>.None;

    public static IOption<T> ToOption<T>(this T? t)
    {
        return t != null ? Some(t) : None<T>();
    }

    public static IOption<(T, R)> Combine<T, R>(this IOption<T> first, IOption<R> second)
    {
        return first.SelectMany(f => second.Select(s => (f, s)));
    }

    // ReSharper disable ParameterTypeCanBeEnumerable.Global
    // ReSharper disable ReturnTypeCanBeEnumerable.Global
    public static IOption<TResult> SelectMany<T, TResult>(this IOption<T> option, Func<T, IOption<TResult>> func)
    {
        return option switch
        {
            Some<T>(var t) => func(t),
            None<T> => new None<TResult>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static IOption<TResult> Select<T, TResult>(this IOption<T> option, Func<T, TResult> func)
    {
        return option switch
        {
            Some<T>(var t) => new Some<TResult>(func(t)),
            None<T> => new None<TResult>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static T GetOrElse<T>(this IOption<T> option, T defaultValue)
    {
        return option switch
        {
            Some<T>(var t) => t,
            None<T> => defaultValue,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static T Get<T>(this IOption<T> option)
    {
        return option switch
        {
            Some<T>(var t) => t,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public record Some<T>(T Value) : IOption<T>;

public record None<T> : IOption<T>;