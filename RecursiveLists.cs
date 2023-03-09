#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/RecursiveLists.cs
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

public interface IRecursiveList<out T> : IEnumerable<T>
{
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        switch (this)
        {
            case Cons<T>(var element, var rest):
                yield return element;
                foreach (var item in rest)
                {
                    yield return item;
                }
                break;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static IRecursiveList<T> operator +(T element, IRecursiveList<T> list)
    {
        return new Cons<T>(element, list);
    }
}

public record Cons<T>(T Element, IRecursiveList<T> Rest) : IRecursiveList<T>;

public record Nil<T> : IRecursiveList<T>;

public static class RecursiveLists
{
    public static IRecursiveList<T> Cons<T>(T element, IRecursiveList<T> rest) => new Cons<T>(element, rest);

    public static IRecursiveList<T> Nil<T>() => new Nil<T>();
}