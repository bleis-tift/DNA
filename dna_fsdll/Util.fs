module Util

open System
open System.Linq.Expressions
open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.FSharp.Reflection
open Microsoft.CSharp.RuntimeBinder

let convert (inp: obj): 'res = 
  inp |> unbox

module NonCached =
  let (?)  (o: obj) (s: string): 'a  = 
    let ty = o.GetType()
    let aty = typeof<'a>
    if not (FSharpType.IsFunction aty) then 
      let site =
        let argInfo = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
        CallSite<Func<CallSite, obj, obj>>.Create(Binder.GetMember(CSharpBinderFlags.None, s, null, [| argInfo |]))
      convert (site.Target.Invoke(site, o))
    else
      let dty, rty = FSharpType.GetFunctionElements aty
      let dtys = 
        if FSharpType.IsTuple dty then FSharpType.GetTupleElements dty 
        elif dty = typeof<unit> then [| |]
        else [| dty |]
      let objToObjFunction = 
        begin fun argObj ->
          let realArgs = 
            match dtys with 
            | [| |] -> [| |]
            | [| _ |] -> [| argObj |]
            | argTys -> 
                assert FSharpType.IsTuple(argObj.GetType())
                FSharpValue.GetTupleFields(argObj)

          let fty = Expression.GetFuncType [| yield typeof<CallSite>; yield typeof<obj>; yield! dtys; yield typeof<obj> |]
          let cty = typedefof<CallSite<_>>.MakeGenericType [| fty |]
          let flag = BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.InvokeMethod
          let site = cty.InvokeMember("Create", flag, null, null, [|(box(Binder.InvokeMember(CSharpBinderFlags.None, s, null, null, Array.create (realArgs.Length + 1) (CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, s)))))|])
                     |> unbox<CallSite>
          let target = site.GetType().GetField("Target").GetValue(site)
          let flag = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.InvokeMethod
          let res = target.GetType().InvokeMember("Invoke", flag, null, target, [| yield box site; yield box o; yield! realArgs |])
          res
        end
      let atyFunction = FSharpValue.MakeFunction(aty,objToObjFunction)
      unbox<'a> atyFunction

  let (?<-) (o : obj) (s : string) (v : 'a) : unit =
    let site = CallSite<Func<CallSite, obj, obj, obj>>.Create(Binder.SetMember(CSharpBinderFlags.None, s, null,  [| CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)|]))
    site.Target.Invoke(site, o, v) |> ignore

open System.Collections.Generic

module Cached =
  let private rcache = Dictionary<Type * string, CallSite>()

  let (?)  (o: obj) (s: string): 'a  = 
    let ty = o.GetType()
    let aty = typeof<'a>
    if not (FSharpType.IsFunction aty) then 
      let site =
        if not (rcache.ContainsKey(ty, s)) then
          let argInfo = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
          rcache.[(ty, s)] <- CallSite<System.Func<CallSite, obj, obj>>.Create(Binder.GetMember(CSharpBinderFlags.None, s, null, [| argInfo |]))
        rcache.[(ty, s)] :?> CallSite<Func<CallSite, obj, obj>>
      convert (site.Target.Invoke(site, o))
    else
      let dty, rty = FSharpType.GetFunctionElements aty
      let dtys = 
        if FSharpType.IsTuple dty then FSharpType.GetTupleElements dty 
        elif dty = typeof<unit> then [| |]
        else [| dty |]
      let objToObjFunction = 
        begin fun argObj ->
          let realArgs = 
            match dtys with 
            | [| |] -> [| |]
            | [| _ |] -> [| argObj |]
            | argTys -> 
                assert FSharpType.IsTuple(argObj.GetType())
                FSharpValue.GetTupleFields(argObj)

          let site =
            if not (rcache.ContainsKey(ty, s)) then
              let fty = Expression.GetFuncType [| yield typeof<CallSite>; yield typeof<obj>; yield! dtys; yield typeof<obj> |]
              let cty = typedefof<CallSite<_>>.MakeGenericType [| fty |]
              let flag = BindingFlags.Public ||| BindingFlags.Static ||| BindingFlags.InvokeMethod
              rcache.[(ty, s)] <- cty.InvokeMember("Create", flag, null, null, [|(box(Binder.InvokeMember(CSharpBinderFlags.None, s, null, null, Array.create (realArgs.Length + 1) (CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, s)))))|])
                                  |> unbox<CallSite>
            rcache.[(ty, s)]
          let target = site.GetType().GetField("Target").GetValue(site)
          let flag = BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.InvokeMethod
          let res = target.GetType().InvokeMember("Invoke", flag, null, target, [| yield box site; yield box o; yield! realArgs |])
          res
        end
      let atyFunction = FSharpValue.MakeFunction(aty,objToObjFunction)
      unbox<'a> atyFunction

  let private wcache = Dictionary<Type * string, CallSite<Func<CallSite, obj, obj, obj>>>()

  let (?<-) (o : obj) (s : string) (v : 'a) : unit =
    let site =
      let ty = o.GetType()
      if not (wcache.ContainsKey(ty, s)) then
        wcache.[(ty, s)] <- CallSite<Func<CallSite, obj, obj, obj>>.Create(Binder.SetMember(CSharpBinderFlags.None, s, null,  [| CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)|]))
      wcache.[(ty, s)]
    site.Target.Invoke(site, o, v) |> ignore