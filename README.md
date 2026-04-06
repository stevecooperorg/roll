# Dice expressions (Rust + C#)

This is a dice roller that lets you write dice expressions like `2d6+1`, `1d20`, or `3d8-2`. There's a rust and a C# version. 

Both programs print something like this:    

```
$ roll 2d6+1

2d6+1 = 3 + 4 + 1 = 
8
```

The project is an illustration -- how can you parse and roll dice expressions in both Rust and C#, without using regex? You could use regex for this, but if you want to parse more complex expressions like whole programming languages, regular expressions get messy fast.

The C# version uses [Superpower](https://github.com/datalust/superpower), a libary that uses LINQ-style syntax to build up complex expressions from smaller parts. It's in the `csharp/` folder.

The rust version uses [Pest](https://pest.rs/) to parse the expression, which uses a special language called a 'grammar' to describe the expression. It's in the `rs/` folder.

# Run the C# version

```bash
dotnet run --project csharp/src/Roll/Roll.csproj -- 2d6+1
```

# Run the rust version

```bash
cargo run --manifest-path rs/Cargo.toml --bin roll -- 2d6+1
```

# Rust: How does Pest work?

We want to parse dice expressions like `2d6+1`, `1d20`, or `3d8-2`.

That means we need to look for a `count`, a `sides`, and an optional `modifier` - so `2d6+1` means 2 dice, with 6 sides, plus 1 modifier.

You could use a regex for this example, but that runs out for more complex expressions. A **parser** fits better: you describe the syntax you want, get the parser to have a go, and if the language is good, then read the **named parts** in code.

[pest.rs](https://pest.rs/) does that with a **grammar file** like this;

```pest
positive_int = { '1'..'9' ~ ('0'..'9')* }
number = { ('0'..'9')+ }
modifier = { ("+" | "-") ~ number }
roll = { count ~ "d" ~ sides ~ modifier? ~ EOI }
count = { positive_int }
sides = { positive_int }
```

This provides names for all the 'bits' of an expression:

- `positive_int` is a number between 1 and 9 followed by 0-9*
- `number` is a number between 0 and 9
- `modifier` is a plus or minus followed by a number
- `roll` is a `count` followed by a `d` followed by a `sides` followed by a `modifier` (if any) followed by end of input
- `count` is a `positive_int`
- `sides` is a `positive_int`

In rust you need to wire together the pest file and the rust code with a special `#[grammar]` attribute:

```rust
#[derive(Parser)]
#[grammar = "dice.pest"]
struct DiceParser;
```

And then you need to call the parser, and it it works write code to extract out the named parts. In [`rs/src/lib.rs`](rs/src/lib.rs) you'll see the `parse_dice()` function, and that returns a `DiceRoll` struct;

```rust
struct DiceRoll {
    count: u32,
    sides: u32,
    modifier: i32,
}
```

You can now use the `DiceRoll` struct to roll the dice, and print the result.

```rust
let roll = parse_dice("2d6+1").unwrap();
let (faces, total) = roll.roll_with_faces(&mut thread_rng());
println!("{}", format_roll_line(&expr, &faces, roll.modifier));
println!("{total}");
```

# C#: How does Superpower work?

[Superpower](https://github.com/datalust/superpower) lets you build parsers by combining smaller parsers together into bigger ones using LINQ-style syntax. 

So you might have a parser for a number like this:

```csharp
// A parser for a number between 1 and 9 followed by 0-9*
private static readonly TextParser<int> PositiveInt =
    from first in Character.In('1', '2', '3', '4', '5', '6', '7', '8', '9')
    from rest in Character.Digit.Many()
    select FoldPositiveDigits(first, rest);
```

And a parser for a modifier like this:

```csharp
    // A parser for a roll like 2d6+1
    private static readonly TextParser<(int Count, int Sides, int Modifier)> Roll =
        from count in PositiveInt
        from _ in D
        from sides in PositiveInt
        from mod in Combinators.OptionalOrDefault(Modifier, 0)
        select (count, sides, mod);
```

And you can see how the `Roll` parser is built from the `PositiveInt` parser and the `Modifier` parser.

It does the same job as the Pest grammar above, but uses a different approach that's much more natural for C#. The implementation lives in `[csharp/src/DiceExpressions.Core/DiceParser.cs](csharp/src/DiceExpressions.Core/DiceParser.cs)`—open that file to see how each rule is wired.