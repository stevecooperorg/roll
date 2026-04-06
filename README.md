# Dice expressions (Rust + C#)

This is a dice roller that supports parsing and rolling dice expressions like `2d6+1`, `1d20`, or `3d8-2`.

The rust version uses [Pest](https://pest.rs/) to parse the expression, while the C# version uses a small hand-written parser.

Both programs print something like this:    

```
$ roll 2d6+1

2d6+1 = 3 + 4 + 1 = 
8
```

The rust is in `rs/`, the C# is in `csharp/`.

## Quick start

**Rust** (from repo root):

```bash
cargo run --manifest-path rs/Cargo.toml --bin roll -- 2d6+1
```

**C#** (.NET 9 SDK; projects target `net9.0`):

```bash
dotnet run --project csharp/src/Roll/Roll.csproj -- 2d6+1
```

## How does Pest work? 

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

And then you need to call the parser, and it it works write code to extract out the named parts. In `rs/src/lib.rs` you'll see the `parse_dice()` function, and that returns a `DiceRoll` struct;

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

### C#: similar jobs, different tools

If you want to write a parser in C# you have a few options:

- Use a parser combinator library like [Superpower](https://github.com/datalust/superpower) or [Sprache](https://github.com/sprache/Sprache)
- Use a grammar file like [Pegasus](https://github.com/otac0n/Pegasus)

For everyday string parsing in C#, **Superpower** or **Sprache** are common first picks. For a dedicated grammar file like Pest, **Pegasus** is the closer analogy.

The **C# sample in this repo** uses a small hand-written parser (no extra NuGet packages) so the logic stays easy to read alongside the Rust grammar; you could port the same shape to Pegasus if you want a `.peg` file.