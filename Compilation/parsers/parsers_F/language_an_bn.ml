(*
The language a^n b^n, i.e. {"", "ab", "aabb", "aaabbb",...}: 
the parsing result "found" field is n, i.e. the word length divided by two.

Implementation idea: 
If you find "a" at index i, 
then: search recursively for a^{n-1}b^{n-1} starting at index i+1,
else: search (and immediately find - assuming the index_from < String.length s) the empty word "". 

Once you found recursively a^{n-1}b^{n-1} (or ""), find "b" at the index_to of the a^{n-1}b^{n-1} 
("b" to match the initial "a"). 

Note: searching recursively means that our parser will be recursive.

The recursive assumption is that the "found" field of a^{n-1}b^{n-1} is {n-1}, so we need to apply
pack to add one to it (accounting for the new "a" & "b"). The base case is the empty word, for which 
the "found" field should be 0, so that too requires a pack operation.

*)

(* Form 1:  *)
#use "full/path/to/pc.ml";;
open PC;;

let rec nt s i =
  let nt1 = caten (word "a") nt in
  let nt1 = pack nt1 (fun (_, n) -> 1 + n) in
  let nt1 = caten nt1 (word "b") in
  let nt1 = (pack nt1 (fun (n, _) -> n)) in
  let nt1 = disj nt1 (pack (word "") (fun _ -> 0)) in
  nt1 s i

(* Form 2 (code equivalent), without the step-by-step approach: *)
let rec nt s i =
  (disj
     (pack (caten
              (pack
                 (caten (word "a") nt)
                 (fun (_, n) -> 1 + n))
              (word "b")) (fun (n, _) -> n))
     (pack (word "") (fun _ -> 0))
  ) s i;;

(* TEST: *)
(nt "" 0);;
(nt "ab" 0);;
(nt "aabb" 0);;
(nt "aaabbb" 0);;

(nt "aab" 0);;
(nt "abb" 0);;

let get_n_of_an_bn s =
  (nt s 0).found