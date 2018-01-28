module Sodium.FSharp.Tests.TransactTests

open FsCheck.Xunit
open Swensen.Unquote
open Sodium.FSharp

[<Property>]
let ``Run construct`` (x : int) (f : int -> int) =
    Stream.sinkWith (Stream.map f)
    |> TestListen.act Stream.listen Transact.runConstruct (Stream.send x)
    |> (=!) [f x]
