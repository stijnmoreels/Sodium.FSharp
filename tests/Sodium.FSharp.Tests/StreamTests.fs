module Sodium.FSharp.Tests.StreamTests

open FsCheck
open FsCheck.Xunit
open Sodium.FSharp
open Swensen.Unquote

[<Property>]
let ``Test send`` (x : int) =
    Stream.sinkWith id
    |> TestListen.stream (Stream.send x)
    |> (=!) [x]

[<Property>]
let ``Test map`` (f : int -> int) x =
    Stream.sinkWith (Stream.map f)
    |> TestListen.stream (Stream.send x)
    |> (=!) [f x]

[<Property>]
let ``Test OrElse Non Simultaneous`` 
    (xs : int list)
    (ys : int list) =
    Stream.sinkWith2 Stream.orElse
    |> TestListen.stream (fun (s1, s2) ->
        Stream.sendAll xs s1 |> ignore
        Stream.sendAll ys s2 |> ignore)
    |> (=!) (xs @ ys)

[<Property>]
let ``Test or else left bias`` (xs : int list) (f : int -> int) =
    Stream.sinkWith (fun s -> Stream.map f s |> Stream.orElse s)
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) (List.map f xs)

[<Property>]
let ``Test coalensce`` (x : int) (y : int) =
    Stream.sinkCoalesceWith (+) id
    |> TestListen.stream 
        (Transact.wrap (Stream.send x >> Stream.send y))
    |> (=!) [x + y]

[<Property>]
let ``Test filter`` (f : char -> bool) (xs : char list) =
    Stream.sinkWith (Stream.filter f)
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) (List.filter f xs)

[<Property>]
let ``Test filter option`` (xs : string option list) =
    Stream.sinkWith Stream.filterOption
    |> TestListen.stream (Stream.sendAll xs)
    |> List.map Some
    |> (=!) (List.filter Option.isSome xs)

[<Property>]
let ``Test gate`` (x : bool) (xs : (char * bool) list) =
    let gate = Cell.sink x
    Stream.sinkWith (Stream.gate gate)
    |> TestListen.stream (fun s ->
        List.iter (fun (x, y) ->
        Stream.send x s |> ignore
        Cell.send y gate |> ignore) xs)
    |> (=!) (TestOracle.gate x xs)

[<Property>]
let ``Test calm`` (xs : int list) =
    Stream.sinkWith Stream.calm
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) (TestOracle.calm xs)

[<Property>]
let ``Test accum`` (x : int) (xs : int list) =
    Stream.sinkWith (Stream.accum x (+))
    |> TestListen.cell (Stream.sendAll xs)
    |> (=!) (TestOracle.accum x xs)

[<Property>]
let ``Test once`` (xs : NonEmptyArray<int>) =
    let xs = Array.toList xs.Get
    Stream.sinkWith Stream.once
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) [List.head xs]

[<Property>]
let ``Test hold`` (x : char) (xs : char list) =
    Stream.sinkWith (fun s -> 
    Stream.snapshotDiscrete (Stream.hold x s) s)
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) (x :: xs |> List.rev |> List.tail |> List.rev)

[<Property>]
let ``Test defer`` (x : char) (xs : char list) =
    Stream.sinkWith (fun s -> 
    Operation.defer s 
    |> Stream.snapshotDiscrete (Stream.hold x s))
    |> TestListen.stream (Stream.sendAll xs)
    |> (=!) xs
    
[<Property>]
let ``Test listen weak`` (xs : int list) (ys : int list) =
    Stream.sinkWith id
    |> fun (sink, stream) -> 
    let out = TestListen.act Stream.listenWeak Transact.none (Stream.sendAll xs) (sink, stream)
    Stream.sendAll ys sink |> ignore
    out =! xs

let tee f x = f x |> ignore; x

[<Property>]
let ``Test listen once async modify`` (xs : NonEmptyArray<char>) =
    let xs = xs.Get |> List.ofArray
    Stream.sinkWith Stream.listenOnceAsync
    |> tee (fst >> Stream.sendAll xs)
    |> snd
    |> Async.RunSynchronously
    |> (=!) (List.head xs)
    
[<Property>]
let ``Test Stream Loop`` () =
    Stream.sinkWith <| fun s ->
        Transact.run <| fun () ->
            let loop = Stream.loop<int>
            Stream.map ((+) 2) loop
            |> Stream.hold 0
            |> Stream.snapshotDiscreteMap (+) <| s
            |> StreamLoop.run loop
    |> TestListen.stream (Stream.sendAll [3; 4; 7; 8])
    |> (=!) [3; 9; 18; 28]

[<Property>]
let ``Test Stream Loop Defer`` () =
    Stream.sinkWith <| fun s ->
        Transact.run <| fun () ->
            let loop = Stream.loop<int>
            Stream.orElse s loop
            |> Stream.filter (fun x -> x < 5)
            |> Stream.map ((+) 1)
            |> Operation.defer
            |> StreamLoop.run loop
    |> TestListen.stream (Stream.send 2)
    |> (=!) [3; 4; 5]