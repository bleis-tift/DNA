module Combination

let ll2a2 xss =
  let a2 = Array2D.zeroCreate (xss |> List.length) (xss |> List.head |> List.length)
  xss |> List.mapi (fun i xs -> xs |> List.mapi (fun j x -> i, j, x))
      |> List.concat
      |> List.iter (fun (i, j, x) -> a2.[i, j] <- x)
  a2

let combIf cond xss =
  // 補助用関数
  //   要素があるときは、先頭要素をばらして再帰呼び出し
  //   要素が無いときは、
  //     計算データ(tmp)が条件を満たせば反転してリストに包む
  //     満たさなければ空のリストを返す
  let rec combIf' cond tmp = function
  | xs::xss -> xs |> List.collect (fun x -> combIf' cond (x::tmp) xss)
  | [] when cond tmp -> [tmp |> List.rev]
  | [] -> []
  
  // 組み合わせ探索
  xss |> combIf' cond []

open ExcelDna.Integration

let AnsCount(range: obj[,], trg: int) =
  // 二次元配列をint list listに変換
  let range =
    [0..range.GetLength(1) - 1]
    |> List.map (fun i -> [ for x in range.[*, i..i] -> x :?> float |> int ])

  // 組み合わせ探索
  range |> combIf (List.sum >> ((=)trg))
        |> List.length