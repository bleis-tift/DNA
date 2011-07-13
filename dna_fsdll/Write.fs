module Write

open System.Diagnostics
open ExcelDna.Integration

let range = [ for r in [1..100] do
              for c in [1..100] -> (r, c) ]

open Util.NonCached

[<ExcelCommand(MenuName="write(F#)", MenuText="COM Object without Cache")>]
let writeNonCachedCom () =
  let sw = Stopwatch.StartNew()

  let excel = ExcelDnaUtil.Application
  let sheet = excel?ActiveSheet
  range
  |> List.iter (fun (row, col) ->
       let cell = sheet?Cells?Item(row, col)
       cell?Value2 <- 1
     ) 

  sw.Stop()
  excel?StatusBar <- sw.Elapsed.TotalSeconds

open Util.Cached

[<ExcelCommand(MenuName="write(F#)", MenuText="COM Object with Cache")>]
let writeCachedCom () =
  let sw = Stopwatch.StartNew()

  let excel = ExcelDnaUtil.Application
  let sheet = excel?ActiveSheet
  range
  |> List.iter (fun (row, col) ->
       let cell = sheet?Cells?Item(row, col)
       cell?Value2 <- 1
     ) 

  sw.Stop()
  excel?StatusBar <- sw.Elapsed.TotalSeconds

[<ExcelCommand(MenuName="write(F#)", MenuText="DNA Object")>]
let writeDna () =
  let sw = Stopwatch.StartNew()

  range
  |> List.iter (fun (row, col) ->
       let row, col = row - 1, col - 1
       let cell = ExcelReference(row, col)
       cell.SetValue(1) |> ignore
     ) 

  sw.Stop()
  ExcelDnaUtil.Application?StatusBar <- sw.Elapsed.TotalSeconds