module Read

open System.Diagnostics
open ExcelDna.Integration

open Util.Cached

let range = [ for r in [1..500] do
              for c in [1..200] -> (r, c) ]

let prepare() =
  range
  |> List.iter (fun (row, col) ->
       let row, col = row - 1, col - 1
       let cell = ExcelReference(row, col)
       cell.SetValue(1) |> ignore
     ) 

[<ExcelCommand(MenuName="read", MenuText="COM Object")>]
let readCom() =
  do prepare()
  let sw = Stopwatch.StartNew()

  let excel = ExcelDnaUtil.Application
  let cells = excel?ActiveSheet?Cells
  let res =
    range
    |> List.sumBy (fun (row, col) ->
         let item = cells?Item(row, col)
         item?Text |> string |> int)

  sw.Stop()
  cells?Item(1, 1)?Value2 <- res
  excel?StatusBar <- sw.Elapsed.TotalSeconds

[<ExcelCommand(MenuName="read", MenuText="DNA Object")>]
let readDna () =
  do prepare()
  let sw = Stopwatch.StartNew()

  let res =
    range
    |> List.sumBy (fun (row, col) ->
        ExcelReference(row - 1, col - 1).GetValue() :?> double)

  sw.Stop()
  ExcelReference(0, 0).SetValue(res) |> ignore
  ExcelDnaUtil.Application?StatusBar <- sw.Elapsed.TotalSeconds