using System.Text;

namespace DiceExpressions;

public record DiceRoll(int Count, int Sides, int Modifier)
{
    public const int MaxCount = 999;
    public const int MaxSides = 999;

    /// <summary>Face of each die and total (dice sum + modifier).</summary>
    public (int[] Faces, int Total) RollWithFaces(Random rng)
    {
        var faces = new int[Count];
        var sum = 0;
        for (var i = 0; i < Count; i++)
        {
            faces[i] = rng.Next(1, Sides + 1);
            sum += faces[i];
        }
        return (faces, sum + Modifier);
    }

    public int Roll(Random rng) => RollWithFaces(rng).Total;

    /// <summary>Human-readable breakdown for stderr, e.g. <c>2d6 = 3 + 4 = </c>.</summary>
    public static string FormatRollLine(string expression, int[] faces, int modifier)
    {
        var sb = new StringBuilder();
        sb.Append(expression).Append(" = ");
        sb.Append(string.Join(" + ", faces));
        if (modifier != 0)
        {
            if (modifier > 0)
                sb.Append(" + ").Append(modifier);
            else
                sb.Append(" - ").Append(-modifier);
        }
        sb.Append(" = ");
        return sb.ToString();
    }
}
