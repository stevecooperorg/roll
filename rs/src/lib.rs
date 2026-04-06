//! Parse dice notation and roll dice.

use pest::iterators::Pair;
use pest::Parser;
use pest_derive::Parser;
use rand::Rng;
use std::fmt;

/// Maximum dice count and face count accepted after parsing (defense in depth).
pub const MAX_COUNT: u32 = 999;
pub const MAX_SIDES: u32 = 999;

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct DiceRoll {
    pub count: u32,
    pub sides: u32,
    pub modifier: i32,
}

#[derive(Debug)]
pub struct ParseDiceError(String);

impl fmt::Display for ParseDiceError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        self.0.fmt(f)
    }
}

impl std::error::Error for ParseDiceError {}

#[derive(Parser)]
#[grammar = "dice.pest"]
struct DiceParser;

/// Parse a single dice expression such as `2d6+1` or `1d20`.
pub fn parse_dice(input: &str) -> Result<DiceRoll, ParseDiceError> {
    let mut pairs = DiceParser::parse(Rule::roll, input).map_err(|e| ParseDiceError(e.to_string()))?;
    let roll = pairs.next().ok_or_else(|| ParseDiceError("empty parse".to_string()))?;

    let mut count: Option<u32> = None;
    let mut sides: Option<u32> = None;
    let mut modifier: i32 = 0;

    for pair in roll.into_inner() {
        match pair.as_rule() {
            Rule::count => {
                let v = parse_u32(pair)?;
                if !(1..=MAX_COUNT).contains(&v) {
                    return Err(ParseDiceError(format!(
                        "count must be between 1 and {MAX_COUNT}"
                    )));
                }
                count = Some(v);
            }
            Rule::sides => {
                let v = parse_u32(pair)?;
                if !(2..=MAX_SIDES).contains(&v) {
                    return Err(ParseDiceError(format!(
                        "sides must be between 2 and {MAX_SIDES}"
                    )));
                }
                sides = Some(v);
            }
            Rule::modifier => {
                modifier = parse_modifier(pair)?;
            }
            Rule::EOI => {}
            _ => {}
        }
    }

    Ok(DiceRoll {
        count: count.ok_or_else(|| ParseDiceError("missing count".to_string()))?,
        sides: sides.ok_or_else(|| ParseDiceError("missing sides".to_string()))?,
        modifier,
    })
}

fn parse_u32(pair: Pair<Rule>) -> Result<u32, ParseDiceError> {
    pair.as_str()
        .parse()
        .map_err(|_| ParseDiceError(format!("invalid number: {:?}", pair.as_str())))
}

fn parse_modifier(pair: Pair<Rule>) -> Result<i32, ParseDiceError> {
    let s = pair.as_str();
    s.parse::<i32>()
        .map_err(|_| ParseDiceError(format!("invalid modifier: {s:?}")))
}

impl DiceRoll {
    /// Roll each die, return face values and total (dice sum + modifier).
    pub fn roll_with_faces<R: Rng + ?Sized>(&self, rng: &mut R) -> (Vec<u32>, i32) {
        let mut faces = Vec::with_capacity(self.count as usize);
        for _ in 0..self.count {
            faces.push(rng.gen_range(1..=self.sides));
        }
        let dice_sum: i32 = faces.iter().map(|&x| x as i32).sum();
        let total = dice_sum + self.modifier;
        (faces, total)
    }

    /// Sum of dice rolls plus modifier, using `rng` for randomness.
    pub fn roll<R: Rng + ?Sized>(&self, rng: &mut R) -> i32 {
        self.roll_with_faces(rng).1
    }
}

/// Human-readable breakdown for stderr: `2d6 = 3 + 4 = ` or `2d6+1 = 3 + 4 + 1 = `.
pub fn format_roll_line(expression: &str, faces: &[u32], modifier: i32) -> String {
    let mut s = format!("{expression} = ");
    let faces_str = faces
        .iter()
        .map(|n| n.to_string())
        .collect::<Vec<_>>()
        .join(" + ");
    s.push_str(&faces_str);
    if modifier != 0 {
        if modifier > 0 {
            s.push_str(&format!(" + {modifier}"));
        } else {
            s.push_str(&format!(" - {}", -modifier));
        }
    }
    s.push_str(" = ");
    s
}

#[cfg(test)]
mod tests {
    use super::*;
    use rand::rngs::StdRng;
    use rand::SeedableRng;

    #[test]
    fn parses_examples() {
        let r = parse_dice("2d6+1").unwrap();
        assert_eq!(
            r,
            DiceRoll {
                count: 2,
                sides: 6,
                modifier: 1
            }
        );
        assert_eq!(
            parse_dice("1d20").unwrap(),
            DiceRoll {
                count: 1,
                sides: 20,
                modifier: 0
            }
        );
        assert_eq!(
            parse_dice("3d8-2").unwrap(),
            DiceRoll {
                count: 3,
                sides: 8,
                modifier: -2
            }
        );
    }

    #[test]
    fn rejects_invalid() {
        assert!(parse_dice("").is_err());
        assert!(parse_dice("d6").is_err());
        assert!(parse_dice("2d").is_err());
        assert!(parse_dice("2d6+").is_err());
        assert!(parse_dice("2d6+1x").is_err());
        assert!(parse_dice("1d1").is_err());
    }

    #[test]
    fn roll_deterministic() {
        let r = parse_dice("2d6").unwrap();
        let mut rng = StdRng::seed_from_u64(42);
        assert_eq!(r.roll(&mut rng), 6);
    }

    #[test]
    fn format_roll_line_examples() {
        assert_eq!(format_roll_line("2d6", &[3, 4], 0), "2d6 = 3 + 4 = ");
        assert_eq!(format_roll_line("2d6+1", &[3, 4], 1), "2d6+1 = 3 + 4 + 1 = ");
        assert_eq!(format_roll_line("3d8-2", &[5, 5, 2], -2), "3d8-2 = 5 + 5 + 2 - 2 = ");
    }
}
