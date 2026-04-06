namespace DiceExpressions;

public static class DiceParser
{
    public static DiceRoll Parse(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new FormatException("empty input");

        var i = 0;
        var count = ReadPositiveInt(input, ref i);
        if (count < 1 || count > DiceRoll.MaxCount)
            throw new FormatException($"count must be between 1 and {DiceRoll.MaxCount}");

        if (i >= input.Length || char.ToLowerInvariant(input[i]) != 'd')
            throw new FormatException("expected 'd'");
        i++;

        var sides = ReadPositiveInt(input, ref i);
        if (sides < 2 || sides > DiceRoll.MaxSides)
            throw new FormatException($"sides must be between 2 and {DiceRoll.MaxSides}");

        var modifier = 0;
        if (i < input.Length)
        {
            var c = input[i];
            if (c != '+' && c != '-')
                throw new FormatException("expected end of input or a modifier");
            var sign = c == '-' ? -1 : 1;
            i++;
            var modNum = ReadNumber(input, ref i);
            modifier = sign * modNum;
        }

        if (i != input.Length)
            throw new FormatException("unexpected trailing characters");

        return new DiceRoll(count, sides, modifier);
    }

    private static int ReadPositiveInt(string s, ref int i)
    {
        if (i >= s.Length || s[i] < '1' || s[i] > '9')
            throw new FormatException("expected a positive integer");
        long v = s[i] - '0';
        i++;
        while (i < s.Length && s[i] >= '0' && s[i] <= '9')
        {
            v = v * 10 + (s[i] - '0');
            if (v > int.MaxValue)
                throw new FormatException("number too large");
            i++;
        }
        return (int)v;
    }

    private static int ReadNumber(string s, ref int i)
    {
        if (i >= s.Length)
            throw new FormatException("expected digits");
        long v = 0;
        var any = false;
        while (i < s.Length && s[i] >= '0' && s[i] <= '9')
        {
            any = true;
            v = v * 10 + (s[i] - '0');
            if (v > int.MaxValue)
                throw new FormatException("number too large");
            i++;
        }
        if (!any)
            throw new FormatException("expected digits");
        return (int)v;
    }
}
