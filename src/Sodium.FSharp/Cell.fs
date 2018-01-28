namespace Sodium.FSharp

open System
open Sodium

module Cell =

    let constant = Cell.Constant

    let constantLazy = Cell.ConstantLazy

    let loop<'a> = (fun () -> Cell.CreateLoop<'a> ()) ()

    let sink x = (fun () -> Cell.CreateSink x) ()

    let sinkWith x f = 
        let sink = sink x
        let stream = f sink
        sink, stream

    let sinkLazyWith x f =
        let sink = sink x
        sink, lazy f sink

    let sinksWith xs f =
        let sinks = List.map sink xs
        let streams = f sinks
        sinks, streams

    let sinkWith2 x y f =
        let sink1 = sink x
        let sink2 = sink y
        let stream = f sink1 sink2
        (sink1, sink2), stream

    let sinkCoalesce x f = (fun () -> Cell.CreateSink(x, f)) ()

    let sample (c : Cell<_>) =
        c.Sample ()

    let sampleLazy (c : Cell<_>) =
        c.SampleLazy ()

    let send x (c : CellSink<_>) =
        c.Send x; c

    let sendAll xs (c : CellSink<_>) =
        List.iter (fun x -> send x c |> ignore) xs; c

    let map f (c : Cell<_>) = 
        c.Map (Func<_, _> f)

    let (<!>) = map

    let apply fCell (c : Cell<_>) =
        c.Apply (map (fun f -> Func<_, _> f) fCell)

    let (<*>) = apply

    let listen f (c : DiscreteCell<_>) =
        c.Listen (Action<_> f)

    let lift2 f (c1 : Cell<_>) c2 =
        c1.Lift (c2, Func<_, _, _> f)

    let lift3 f (c1 : Cell<_>) c2 c3 =
        c1.Lift (c2, c3, Func<_, _, _, _> f)

    let lift4 f (c1 : Cell<_>) c2 c3 c4 =
        c1.Lift (c2, c3, c4, Func<_, _, _, _, _> f)

    let lift5 f (c1 : Cell<_>) c2 c3 c4 c5 =
        c1.Lift (c2, c3, c4, c5, Func<_, _, _, _, _, _> f)

    let lift6 f (c1 : Cell<_>) c2 c3 c4 c5 c6 =
        c1.Lift (c2, c3, c4, c5, c6, Func<_, _, _, _, _, _, _> f)

module DiscreteCell =

    let sink x = (fun () -> DiscreteCell.CreateSink x) ()

    let sinkWith x f =
        let sink = sink x
        let stream = f sink
        sink, stream

    let sinkLazyWith x f =
        let sink = sink x
        sink, lazy f sink

    let sinkWith2 x y f =
        let sink1 = sink x
        let sink2 = sink y
        let stream = f (sink1, sink2)
        (sink1, sink2), stream

    let lift2 f (c1 : DiscreteCell<_>) c2 =
        c1.Lift (c2, Func<_, _, _> f)

    let map f (c : DiscreteCell<_>) =
        c.Map (Func<_, _> f)

    let (<!>) = map

    let apply fCell (c : DiscreteCell<_>) =
        c.Apply (map (fun f -> Func<_, _> f) fCell)

    let (<*>) = apply

    let gate (c : DiscreteCell<_>) (s : Stream<_>) =
        s.Gate c

    let loop<'a> =
        (fun () -> DiscreteCell.CreateLoop<'a> ()) ()

    let values (c : DiscreteCell<_>) =
        c.Values

    let send x (c : DiscreteCellSink<_>) =
        c.Send x; c

    let sendAll xs (c : DiscreteCellSink<_>) =
        List.iter (fun x -> send x c |> ignore) xs

module DiscreteCellLoop =
    
    let run (c1 : DiscreteCellLoop<_>) c2 =
        c1.Loop c2; c2

