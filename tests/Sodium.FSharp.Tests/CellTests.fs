module Sodium.FSharp.Tests.CellTests

open FsCheck.Xunit
open Swensen.Unquote
open Sodium.FSharp

[<Property>]
let ``Test hold`` (x : int) xs =
    Stream.sinkWith (Stream.hold x)
    |> TestListen.cell (Stream.sendAll xs)
    |> (=!) (x :: xs)

[<Property>]
let ``Test hold updates`` (x : int) xs =
    Stream.sinkWith 
        (Stream.hold x >> Operation.updatesDiscrete)
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) xs

[<Property>]
let ``Test listen once`` (x : int) (y : int) =
    Cell.sinkLazyWith x Operation.value
    |> TestListen.actLazy Stream.listenOnce Transact.run (Cell.send y)
    |> (=!) [x]

[<Property>]
let ``Test updates`` (x : int) (xs : int list) =
    Cell.sinkWith x Operation.updates
    |> TestListen.stream (Cell.sendAll xs)
    |> (=!) xs

[<Property>]
let ``Test value`` (x : int) (xs : int list) =
    Cell.sinkLazyWith x Operation.value
    |> TestListen.streamTransactLazy Transact.run (Cell.sendAll xs)
    |> (=!) (x :: xs)

[<Property>]
let ``Test discrete cell loop`` () =
    Stream.sinkWith <| fun s ->
        Transact.run <| fun () ->
            let loop = DiscreteCell.loop<int>
            Stream.snapshotDiscreteMap (+) loop s
            |> Stream.hold 1
            |> DiscreteCellLoop.run loop
    |> TestListen.cell (Stream.sendAll [3; 4; 7; 8])
    |> (=!) [1; 4; 8; 15; 23]

[<Property>]
let ``Test value then merge`` () =
    Cell.sinkWith2 9 2 (fun c1 c2 -> 
        Operation.value c1 
        |> Stream.merge (+) (Operation.value c2))
        |> TestListen.streamTransact Transact.run (fun (c1, c2) ->
            Cell.send 1 c1 |> ignore
            Cell.send 4 c2 |> ignore
            Transact.runVoid <| fun () ->
                Cell.send 7 c1 |> ignore
                Cell.send 5 c2 |> ignore)
        |> (=!) [1; 4; 12]

[<Property>]
let ``Test value then filter`` (x : int) (xs : int list) (f : int -> bool) =
    Cell.sinkLazyWith x (Operation.value >> Stream.filter f)
    |> TestListen.streamTransactLazy Transact.run (Cell.sendAll xs)
    |> (=!) (x :: xs |> List.filter f)

[<Property>]
let ``Test discrete cell values then map`` (x : int) (f : int -> string) (y : int) =
    DiscreteCell.sinkLazyWith x (DiscreteCell.values >> Stream.map f)
    |> TestListen.streamTransactLazy Transact.run (DiscreteCell.send y)
    |> (=!) [f x; f y]

[<Property>]
let ``Test discrete cell values then filter`` (x : int) (f : int -> bool) (y : int) =
    DiscreteCell.sinkLazyWith x (DiscreteCell.values >> Stream.filter f)
    |> TestListen.streamTransactLazy Transact.run (DiscreteCell.send y)
    |> (=!) ([x; y] |> List.filter f)

[<Property>]
let ``Test map`` (x : int) (y : int) (f : int -> string) =
    DiscreteCell.sinkWith x (DiscreteCell.map f)
    |> TestListen.cell (DiscreteCell.send y)
    |> (=!) [f x; f y]

[<Property>]
let ``Test apply`` x y (f : int -> int) =
    DiscreteCell.sinkWith x (DiscreteCell.apply (DiscreteCell.sink f))
    |> TestListen.cell (DiscreteCell.send y)
    |> (=!) [f x; f y]

[<Property>]
let ``Test lift`` (init1 : int) (init2 : int) (f : int -> int -> int) (x : int) =
    DiscreteCell.sinkWith init1 (fun s -> 
        DiscreteCell.lift2 f s (DiscreteCell.sink init2))
    |> TestListen.cell (DiscreteCell.send x)
    |> (=!) [f init1 init2; f x init2]   