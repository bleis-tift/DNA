module CombinationScenario

open NaturalSpec

let supermabInput = 
  [ [ 0; 1; 4;  8; 11; 25; 32 ]
    [ 0; 2; 5; 10; 16; 47; 51 ]
    [ 0; 3; 6; 14; 29; 59; 74 ]
    [ 0; 4; 7; 17; 45; 61; 87 ] ]

[<Scenario>]
let 答えが一つもない例() =
  Given supermabInput
  |> When Combination.combIf (List.sum >> ((=) -1))
  |> It should equal []
  |> Verify

[<Scenario>]
let 答えが一つしかない例() =
  Given supermabInput
  |> When Combination.combIf (List.sum >> ((=)0))
  |> It should equal [[0; 0; 0; 0]]
  |> Verify

[<Scenario>]
let supermabさんの例() =
  Given supermabInput
  |> When Combination.combIf (List.sum >> ((=)100))
  |> It should equal [ [  0; 10;  3; 87 ]
                       [  0; 10; 29; 61 ]
                       [  1; 51;  3; 45 ]
                       [  4;  5; 74; 17 ]
                       [  4; 51;  0; 45 ]
                       [  8;  2;  3; 87 ]
                       [  8;  2; 29; 61 ]
                       [  8;  5;  0; 87 ]
                       [  8; 16; 59; 17 ]
                       [  8; 47;  0; 45 ]
                       [ 11;  2;  0; 87 ]
                       [ 25;  0; 14; 61 ]
                       [ 25; 16; 14; 45 ]
                       [ 25; 16; 59;  0 ]
                       [ 32;  2; 59;  7 ]
                       [ 32;  5; 59;  4 ]
                       [ 32; 47; 14;  7 ]
                       [ 32; 51;  0; 17 ] ]
  |> Verify