use rand::thread_rng;
use roll::{format_roll_line, parse_dice};

fn main() {
    let mut args = std::env::args().skip(1).collect::<Vec<_>>();
    if args.len() != 1 {
        eprintln!("usage: roll <expression>");
        eprintln!("example: roll 2d6+1");
        std::process::exit(1);
    }
    let expr = args.pop().unwrap();
    match parse_dice(&expr) {
        Ok(roll) => {
            let (faces, total) = roll.roll_with_faces(&mut thread_rng());
            eprintln!("{}", format_roll_line(&expr, &faces, roll.modifier));
            println!("{total}");
        }
        Err(e) => {
            eprintln!("{e}");
            std::process::exit(1);
        }
    }
}
