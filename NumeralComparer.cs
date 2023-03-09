using System.Text.RegularExpressions;

namespace NumeralSynthesizer;

public sealed partial class NumeralComparer : IEqualityComparer<string>
{
    public static readonly NumeralComparer Strict = new(true);

    public static readonly NumeralComparer NonStrict = new(false);
    
    private readonly bool _strict;

    private NumeralComparer(bool strict)
    {
        _strict = strict;
    }

    public bool Equals(string? x, string? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        var normalizedX = WhitespaceRegex().Replace(x, " ");
        var normalizedY = WhitespaceRegex().Replace(y, " ");
        return _strict
            ? normalizedX == normalizedY
            : normalizedX.Replace("-", " ") == normalizedY.Replace("-", " ");
    }

    public int GetHashCode(string obj)
    {
        return obj.GetHashCode();
    }

    [GeneratedRegex("\\s{2,}")]
    private static partial Regex WhitespaceRegex();
}