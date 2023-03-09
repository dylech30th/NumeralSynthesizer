#region Copyright (c) NumeralSynthesizer/NumeralSynthesizer
// GPL v3 License
// 
// NumeralSynthesizer/NumeralSynthesizer
// Copyright (c) 2023 NumeralSynthesizer/EnumerationHelper.cs
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

public static class EnumerationHelper
{
    public static int LastIndexOf<T>(this IReadOnlyList<T> coll, Func<T, bool> predicate)
    {
        for (var i = coll.Count - 1; i >= 0; i--)
        {
            if (predicate(coll[i]))
            {
                return i;
            }
        }

        return -1;
    }

    public static int IndexOf<T>(this IReadOnlyList<T> coll, Func<T, bool> predicate)
    {
        for (var i = 0; i < coll.Count; i++)
        {
            if (predicate(coll[i]))
            {
                return i;
            }
        }

        return -1;
    }
}