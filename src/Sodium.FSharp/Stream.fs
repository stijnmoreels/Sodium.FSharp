namespace Sodium.FSharp

open System.Threading
open System.Threading.Tasks
open System
open Sodium

module Stream =
    
    let accum init f (s : Stream<_>) =
        s.Accum (init, Func<_, _, _> f)

    let accumLazy init f (s : Stream<_>) =
        s.AccumLazy (init, Func<_, _, _> f)

    let attachListener l (s : Stream<_>) =
        s.AttachListener l

    let calm (s : Stream<_>) =
        s.Calm ()

    let calmCompare c (s : Stream<_>) =
        s.Calm c

    let collect init f (s : Stream<_>) =
        s.Collect (init, Func<_, _, _> (fun x acc -> 
            let (x, y) = f x acc
            struct (x, y)))

    let collectLazy init f (s : Stream<_>) =
        s.CollectLazy (init, Func<_, _, _> (fun x acc -> 
            let (x, y) = f x acc
            struct (x, y)))

    let filter f (s : Stream<_>) =
        s.Filter (Func<_, _> f)

    let filterOption (s : Stream<_>) =
        s.Map(Func<_, _> Option.toMaybe).FilterMaybe ()

    let gate (c : Cell<_>) (s : Stream<_>) =
        s.Gate c

    let hold init (s : Stream<_>) =
        s.Hold init

    let holdLazy init (s : Stream<_>) =
        s.HoldLazy init

    let loop<'a> = 
        (fun () -> Stream.CreateLoop<'a> ()) ()

    let listen f (s : Stream<_>) =
        s.Listen (new Action<_> (f))

    let listenOnce f (s : Stream<_>) =
        s.ListenOnce (new Action<_> (f))

    let listenWeak f (s : Stream<_>) =
        s.ListenWeak (new Action<_> (f))

    let listenOnceAsync (s : Stream<_>) =
        s.ListenOnceAsync().GetAwaiter() |> Async.FromTaskAwaiter

    let listenOnceAsyncCancel (ct : CancellationToken) (s : Stream<_>) =
        s.ListenOnceAsync ct
    
    let listenOnceAsyncModify f (s : Stream<_>) =
        s.ListenOnceAsync (Func<_, _> f)

    let listenOnceAsyncModifyResult f (s : Stream<_>) =
        s.ListenOnceAsync (Func<_, Task<_>> f)

    let listenOnceAsyncCancelModify ct f (s : Stream<_>) =
        s.ListenOnceAsync (Func<_, _> f, ct)

    let listenOnceAsyncCancelModifyResult ct f (s : Stream<_>) =
        s.ListenOnceAsync (Func<_, Task<_>> f, ct)

    let map f (s : Stream<_>) =
        s.Map (Func<_, _> f)

    let mapTo x (s : Stream<_>) =
        s.MapTo x

    let merge f s2 (s1 : Stream<_>) =
        s1.Merge (s2, Func<_, _, _> f)

    let never<'a> = 
        (fun () -> Stream.Never<'a> ()) ()
    
    let once (s : Stream<_>) =
        s.Once ()

    let orElse (s2 : Stream<_>) (s1 : Stream<_>) =
        s1.OrElse s2
    
    let send x (s : StreamSink<_>) =
        s.Send x; s

    let sendAll xs (s : StreamSink<_>) =
        List.iter (fun x -> send x s |> ignore) xs; s

    let sink<'a> = 
        (fun () -> Stream.CreateSink<'a> ()) ()

    let sinkCoalesce<'a> f =
        (fun () -> Stream.CreateSink<'a> (Func<_, _, _> f)) ()

    let sinkCoalesceWith f g =
        let sink = sinkCoalesce<'a> f
        let stream = g sink
        sink, stream

    let sinkWith (f : StreamSink<'a> -> 'b) =
        let sink = sink
        let stream = f sink
        sink, stream

    let sinksWith n (f : StreamSink<'a> list -> 'b) =
        let sinks = List.replicate n sink
        let streams = f sinks
        sinks, streams

    let sinkWith2 (f : StreamSink<'a> -> StreamSink<'b> -> 'c) =
        let sink1 = sink
        let sink2 = sink
        let stream = f sink1 sink2
        (sink1, sink2), stream

    let snapshot (c : Cell<_>) (s : Stream<_>) =
        s.Snapshot c

    let snapshotDiscrete (c : DiscreteCell<_>) (s : Stream<_>) =
        s.Snapshot c

    let snapshotMap f (c : Cell<_>) (s : Stream<_>) =
        s.Snapshot (c, Func<_, _, _> f)

    let snapshotDiscreteMap f (c : DiscreteCell<_>) (s : Stream<_>) =
        s.Snapshot (c, Func<_, _, _> f)

    let snapshot2 f (c1 : Cell<_>) c2 (s : Stream<_>) =
        s.Snapshot (c1, c2, Func<_, _, _, _> f)

    let snapshotDiscrete2 f (c1 : DiscreteCell<_>) c2 (s : Stream<_>) =
        s.Snapshot (c1, c2, Func<_, _, _, _> f)

    let snapshot3 f (c1 : Cell<_>) c2 c3 (s : Stream<_>) =
        s.Snapshot (c1, c2, c3, Func<_, _, _, _, _> f)

    let snapshotDiscrete3 f (c1 : DiscreteCell<_>) c2 c3 (s : Stream<_>) =
        s.Snapshot (c1, c2, c3, Func<_, _, _, _, _> f)

    let snapshot4 f (c1 : Cell<_>) c2 c3 c4 (s : Stream<_>) =
        s.Snapshot (c1, c2, c3, c4, Func<_, _, _, _, _, _> f)

    let snapshotDiscrete4 f (c1 : DiscreteCell<_>) c2 c3 c4 (s : Stream<_>) =
        s.Snapshot (c1, c2, c3, c4, Func<_, _, _, _, _, _> f)

module StreamLoop =

    let run (s1 : StreamLoop<_>) s2 =
        s1.Loop s2; s2

