:- module('ex4',
        [author/2,
         genre/2,
         book/4
        ]).

/*
 * **********************************************
 * Printing result depth
 *
 * You can enlarge it, if needed.
 * **********************************************
 */
maximum_printing_depth(100).
:- current_prolog_flag(toplevel_print_options, A),
   (select(max_depth(_), A, B), ! ; A = B),
   maximum_printing_depth(MPD),
   set_prolog_flag(toplevel_print_options, [max_depth(MPD)|B]).



author(a, asimov).
author(h, herbert).
author(m, morris).
author(t, tolkien).

genre(s, science).
genre(l, literature).
genre(sf, science_fiction).
genre(f, fantasy).

book(inside_the_atom, a, s, s(s(s(s(s(zero)))))).
book(asimov_guide_to_shakespeare, a, l, s(s(s(s(zero))))).
book(i_robot, a, sf, s(s(s(zero)))).
book(dune, h, sf, s(s(s(s(s(zero)))))).
book(the_well_at_the_worlds_end, m, f, s(s(s(s(zero))))).
book(the_hobbit, t, f, s(s(s(zero)))).
book(the_lord_of_the_rings, t, f, s(s(s(s(s(s(zero))))))).

% You can add more facts.


% Signature: max_list(Lst, Max)/2
% Purpose: true if Max is the maximum church number in Lst, false if Lst is emoty.
natural_number(zero).
natural_number(s(X)) :- natural_number(X).

max_nat(zero, Y, Y).
max_nat(X, zero, X) :- X \= zero.
max_nat(s(X), s(Y), s(Max)) :- max_nat(X, Y, Max).

max_list([X], X).

max_list([X|Xs], Max) :-
    max_list(Xs, TempMax),
    max_nat(X, TempMax, Max).


% Signature: author_of_genre(GenreName, AuthorName)/2
% Purpose: true if an author by the name AuthorName has written a book belonging to the genre named GenreName.
author_of_genre(GenreName, AuthorName) :-
    genre(GenreID, GenreName),
    author(AuthorID, AuthorName),
    book(_, AuthorID, GenreID, _).



% Signature: longest_book(AuthorName, BookName)/2
% Purpose: true if the longest book that an author by the name AuthorName has written is titled BookName.
longest_book(AuthorName, BookName):-
    author(AuthorID, AuthorName),
    findall(Length-Name, book(Name, AuthorID, _, Length), BookList),
    max_length_book(BookList, Length-BookName).

max_length_book([X], X).

max_length_book([Length1-Name1, Length2-Name2 | Rest], Max) :-
    max_nat(Length1, Length2, Longer),
    (
        Longer = Length1 ->
        max_length_book([Length1-Name1 | Rest], Max);
        max_length_book([Length2-Name2 | Rest], Max)
    ).