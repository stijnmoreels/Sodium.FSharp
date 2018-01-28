namespace Sodium.FSharp.Tests

module TestListen =
    open System.Collections.Concurrent
    open Sodium.FSharp

    let act listen transact send (sink, s) =
        let xs = new ConcurrentQueue<'a> ()
        let l = transact <| fun () -> listen xs.Enqueue s
        send sink |> ignore
        Listener.unlisten l
        xs.ToArray () |> List.ofArray

    let actLazy listen transact send (sink, s : Lazy<_>) =
        let xs = new ConcurrentQueue<'a> ()
        let l = transact <| fun () -> listen xs.Enqueue s.Value
        send sink |> ignore
        Listener.unlisten l
        xs.ToArray () |> List.ofArray

    let cell send s =
        act Cell.listen Transact.none send s

    let cellTransact transact send s =
        act Cell.listen transact send s

    let cellTransactLazy transact send s =
        actLazy Cell.listen transact send s
    
    let cell2 send1 send2 ((sink1, sink2), c) =
        let xs = cell send1 (sink1, c)
        let ys = cell send2 (sink2, c)
        xs, ys

    let cells sends (sinks, c) =
        List.zip sends sinks
        |> List.map (fun (send, sink) -> cell send (sink, c))

    let stream send s =
        act Stream.listen Transact.none send s

    let streamTransact transact send s =
        act Stream.listen transact send s

    let streamTransactLazy transact send s =
        actLazy Stream.listen transact send s

    let stream2 send1 send2 ((sink1, sink2), s) =
        let xs = stream send1 (sink1, s)
        let ys = stream send2 (sink2, s)
        xs, ys

    let streams sends (sinks, s) =
        List.zip sends sinks
        |> List.map (fun (send, sink) -> stream send (sink, s))

module TestOracle =

    let gate start (xs : ('a * bool) list) =
        start :: List.map snd xs
        |> List.rev 
        |> List.tail 
        |> List.rev
        |> List.zip (List.map fst xs)
        |> List.filter snd 
        |> List.map fst

    let calm list =
        let folder (last, result) x =
            if Option.exists ((=) x) last 
            then Some x, result
            else Some x, x :: result

        List.fold folder (None, []) list
        |> snd
        |> List.rev

    let accum init list =
        let folder (acc, result) x =
            let y = acc + x
            let next = init + y
            y, next :: result

        List.fold folder (0, []) list
        |> snd
        |> fun xs -> init :: (List.rev xs)