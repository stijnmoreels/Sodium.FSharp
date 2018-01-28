module Sodium.FSharp.Tests.CommonTests

open FsCheck.Xunit
open Sodium.FSharp
open Swensen.Unquote

let ``Test base send 1`` (xs : string list) =
    Stream.sinkWith id
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) xs

[<Property>]
let ``Test operation split`` (xs : string list) =
    Stream.sinkWith (Stream.map Seq.ofList >> Operation.split)
    |> TestListen.stream (Stream.send xs)
    |> (=!) xs

[<Property>]
let ``Test operation defer 1`` (x : string) =
    Stream.sinkWith Operation.defer
    |> TestListen.stream (Stream.send x)
    |> (=) [x]

[<Property>]
let ``Test orElse 2`` (x : string) (y : string) =
    Stream.sinksWith 2 (fun (s1 :: s2 :: []) -> s1 |> Stream.orElse s2) 
    |> TestListen.streams [ Stream.send x; Transact.wrap (Stream.send y) ]
    |> (=!) [[x]; [y]]
        
[<Property>]
let ``Test stream or else 1`` (z : int) =
    Stream.sinkWith2 Stream.orElse
    |> TestListen.stream (fun (s1, s2) ->
        Transact.run <| fun () ->
            Stream.send z s1 |> ignore
            Stream.send z s2)
    |> (=!) [z]