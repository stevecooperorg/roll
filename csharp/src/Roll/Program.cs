using DiceExpressions;

if (args.Length != 1)
{
    Console.Error.WriteLine("usage: roll <expression>");
    Console.Error.WriteLine("example: roll 2d6+1");
    Environment.Exit(1);
}

try
{
    var expr = args[0];
    var roll = DiceParser.Parse(expr);
    var (faces, total) = roll.RollWithFaces(Random.Shared);
    Console.Error.WriteLine(DiceRoll.FormatRollLine(expr, faces, roll.Modifier));
    Console.WriteLine(total);
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);
    Environment.Exit(1);
}
