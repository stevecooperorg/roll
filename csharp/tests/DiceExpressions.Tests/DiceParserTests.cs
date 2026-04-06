using DiceExpressions;
using Xunit;

namespace DiceExpressions.Tests;

public class DiceParserTests
{
    [Fact]
    public void Parses_examples()
    {
        Assert.Equal(new DiceRoll(2, 6, 1), DiceParser.Parse("2d6+1"));
        Assert.Equal(new DiceRoll(1, 20, 0), DiceParser.Parse("1d20"));
        Assert.Equal(new DiceRoll(3, 8, -2), DiceParser.Parse("3d8-2"));
    }

    [Fact]
    public void Rejects_invalid()
    {
        Assert.Throws<FormatException>(() => DiceParser.Parse(""));
        Assert.Throws<FormatException>(() => DiceParser.Parse("d6"));
        Assert.Throws<FormatException>(() => DiceParser.Parse("2d"));
        Assert.Throws<FormatException>(() => DiceParser.Parse("2d6+"));
        Assert.Throws<FormatException>(() => DiceParser.Parse("2d6+1x"));
        Assert.Throws<FormatException>(() => DiceParser.Parse("1d1"));
    }

    [Fact]
    public void Roll_seeded_is_repeatable()
    {
        var roll = DiceParser.Parse("2d6");
        var a = roll.Roll(new Random(42));
        var b = roll.Roll(new Random(42));
        Assert.Equal(a, b);
    }

    [Fact]
    public void Format_roll_line_examples()
    {
        Assert.Equal("2d6 = 3 + 4 = ", DiceRoll.FormatRollLine("2d6", new[] { 3, 4 }, 0));
        Assert.Equal("2d6+1 = 3 + 4 + 1 = ", DiceRoll.FormatRollLine("2d6+1", new[] { 3, 4 }, 1));
        Assert.Equal("3d8-2 = 5 + 5 + 2 - 2 = ", DiceRoll.FormatRollLine("3d8-2", new[] { 5, 5, 2 }, -2));
    }
}
