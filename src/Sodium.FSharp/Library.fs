namespace Sodium.FSharp

open System
open Sodium

module Option =

    let ofMaybe (m : Maybe<_>) =
        m.Match (Func<_, _> (fun x -> Some x), Func<_> (fun () -> None))

    let toMaybe = function
        | Some x -> Maybe.Some x
        | None -> Maybe<_>.None

    let toBool = function
        | Some _ -> true
        | None -> false

module Async =
    open System.Runtime.CompilerServices
    open System.Threading

    let FromTaskAwaiter (awaiter: TaskAwaiter<'a>) = async {
            use handle = new SemaphoreSlim(0)
            awaiter.OnCompleted(fun () -> ignore <| handle.Release ())
            let! _ = handle.AvailableWaitHandle |> Async.AwaitWaitHandle
            return awaiter.GetResult () }

module Transact =
    
    let onStart f = Transaction.OnStart (new Action (f))

    let post f = Transaction.Post (new Action (f))

    let run f = Transaction.Run (new Func<_> (f))

    let runConstruct f = Transaction.RunConstruct (new Func<_> (f))

    let runConstructVoid f = Transaction.RunConstructVoid (new Action (f))

    let runVoid f = Transaction.RunVoid (new Action (f))

    let wrap f = fun x ->
        run (fun () -> f x)

    let none f = f () 

module Operation =

    let defer = Operational.Defer

    let split (s : Stream<'a seq>) = Operational.Split s

    let updates = Operational.Updates

    let updatesDiscrete (c : DiscreteCell<_>) = updates c.Cell

    let value = Operational.Value

module Listener =

    let unlisten (l : IListener) =
        l.Unlisten ()