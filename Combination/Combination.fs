module Combination

let combIf cond xss =
  let rec combIf' cond tmp = function
  | xs::xss -> xs |> List.collect (fun x -> combIf' cond (x::tmp) xss)
  | [] when cond tmp -> [tmp |> List.rev]
  | [] -> []
  
  xss |> combIf' cond []
