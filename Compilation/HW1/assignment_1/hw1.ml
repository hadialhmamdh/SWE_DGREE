(*<number> ::= <simple-decimal> | <based-number>

<simple-decimal> ::= <digit>+
                   where digit ∈ {0,1,2,3,4,5,6,7,8,9}

<based-number> ::= <base> "#" <digit-sequence>

<base> ::= <number>
         (recursive: any number can serve as a base)

<digit-sequence> ::= <digit-representation>+

<digit-representation> ::= <simple-digit> | <bracketed-digit>

<simple-digit> ::= <digit> | <letter>
                 where digit ∈ {0-9}
                 and letter ∈ {A-Z, a-z}
                 
<bracketed-digit> ::= <bracket-open> <whitespace>* <number> <whitespace>* <bracket-close>

<bracket-open> ::= "(" | "[" | "{" | "<"

<bracket-close> ::= ")" | "]" | "}" | ">"

<whitespace> ::= " " | "\t" | "\n" | etc.*)

(* hw1.ml *)
#use "pc.ml"
open PC

let char_to_digit_value c =
  let code = Char.code c in
  let code_0 = Char.code '0' in
  let code_9 = Char.code '9' in
  let code_A = Char.code 'A' in
  let code_Z = Char.code 'Z' in
  let code_a = Char.code 'a' in
  let code_z = Char.code 'z' in
  if code >= code_0 && code <= code_9 then
    code - code_0
  else if code >= code_A && code <= code_Z then
    10 + (code - code_A)
  else if code >= code_a && code <= code_z then
    10 + (code - code_a)
  else
    raise (Failure "Invalid digit character")

let char_list_to_int char_list =
  let code_0 = Char.code '0' in
  List.fold_left
    (fun acc c -> acc * 10 + (Char.code c) - code_0)
    0
    char_list

let digits_to_number base digits =
  List.fold_left
    (fun acc digit_val -> acc * base + digit_val)
    0
    digits

let nt_simple_digit =
  let nt_digit = range '0' '9' in
  (* let nt_upper = range 'A' 'Z' in *)
  let nt_lower = range_ci 'a' 'z' in
  let nt = disj nt_digit  nt_lower in
  pack nt char_to_digit_value

(* returns Some c if there is a next character after whitespace, None if input ended *)
let nt_whitespace =
  let nt_ws = disj_list [char ' '; char '\t'; char '\n'; char '\r'] in
  let nt = star nt_ws in
  pack nt (fun _ -> ())
 

let nt_simple_decimal =
  let nt = range '0' '9' in
  let nt = plus nt in
  pack nt char_list_to_int

let rec nt_number str idx =
  (* Start with a simple decimal *)
  let base_result = nt_simple_decimal str idx in
  (* Try to extend it with # and digits as many times as possible *)
  extend_with_hash base_result str

and extend_with_hash result str =
  try
    (* Try to parse # followed by digit sequence *)
    let hash_result = (char '#') str result.index_to in
    let digits_result = nt_digit_sequence str hash_result.index_to in
    (* Calculate the based number *)
    let value = digits_to_number result.found digits_result.found in
    let new_result = {
      index_from = result.index_from;
      index_to = digits_result.index_to;
      found = value
    } in
    extend_with_hash new_result str
  with PC.X_no_match ->
    (* No more # extensions, return what we have *)
    result

and nt_digit_sequence str idx =
  let nt = plus nt_digit_repr in
  nt str idx

and nt_digit_repr str idx =
  let nt = disj nt_bracketed_digit nt_simple_digit in
  nt str idx

and nt_bracketed_digit str idx =
  let make_bracket_parser open_char close_char str idx =
    let nt_open = char open_char in
    let nt_close = char close_char in
    let nt = caten nt_open nt_whitespace in
    let nt = pack nt (fun (_, _) -> ()) in
    let nt = caten nt nt_number in
    let nt = pack nt (fun (_, num) -> num) in
    let nt = caten nt nt_whitespace in
    let nt = pack nt (fun (num, _) -> num) in
    let nt = caten nt nt_close in
    let nt = pack nt (fun (num, _) -> num) in
    nt str idx in
  let nt_paren = make_bracket_parser '(' ')' in
  let nt_square = make_bracket_parser '[' ']' in
  let nt_curly = make_bracket_parser '{' '}' in
  let nt_angle = make_bracket_parser '<' '>' in
  let nt = disj_list [nt_paren; nt_square; nt_curly; nt_angle] in
  nt str idx

