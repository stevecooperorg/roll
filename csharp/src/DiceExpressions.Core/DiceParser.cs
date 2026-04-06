using Superpower;
using Superpower.Parsers;

namespace DiceExpressions;

public static class DiceParser
{
    private static readonly TextParser<int> PositiveInt =
        from first in Character.In('1', '2', '3', '4', '5', '6', '7', '8', '9')
        from rest in Character.Digit.Many()
        select FoldPositiveDigits(first, rest);

    private static readonly TextParser<char> D = Character.EqualToIgnoreCase('d');

    private static readonly TextParser<int> Modifier =
        from sign in Character.EqualTo('+').Or(Character.EqualTo('-'))
        from digits in Character.Digit.AtLeastOnce()
        select Sign(sign) * FoldUnsignedDigits(digits);

    private static readonly TextParser<(int Count, int Sides, int Modifier)> Roll =
        from count in PositiveInt
        from _ in D
        from sides in PositiveInt
        from mod in Combinators.OptionalOrDefault(Modifier, 0)
        select (count, sides, mod);

    private static readonly TextParser<(int Count, int Sides, int Modifier)> RollAtEnd = Combinators.AtEnd(Roll);

    public static DiceRoll Parse(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new FormatException("empty input");

        try
        {
            var (count, sides, modifier) = RollAtEnd.Parse(input);
            if (count < 1 || count > DiceRoll.MaxCount)
                throw new FormatException($"count must be between 1 and {DiceRoll.MaxCount}");
            if (sides < 2 || sides > DiceRoll.MaxSides)
                throw new FormatException($"sides must be between 2 and {DiceRoll.MaxSides}");
            return new DiceRoll(count, sides, modifier);
        }
        catch (ParseException e)
        {
            throw new FormatException(e.Message, e);
        }
    }

    private static int Sign(char c) => c == '-' ? -1 : 1;

    private static int FoldPositiveDigits(char first, char[] rest)
    {
        long v = first - '0';
        foreach (var c in rest)
        {
            v = v * 10 + (c - '0');
            if (v > int.MaxValue)
                throw new FormatException("number too large");
        }
        return (int)v;
    }

    private static int FoldUnsignedDigits(char[] digits)
    {
        long v = 0;
        foreach (var c in digits)
        {
            v = v * 10 + (c - '0');
            if (v > int.MaxValue)
                throw new FormatException("number too large");
        }
        return (int)v;
    }
}
