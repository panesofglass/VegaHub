VegaHub
=======

A SignalR Hub Utility for data scientists to push Vega charts from F# Interactive

Current Status
--------------

Proof-of-Concept

### Available charts

* bar
* colorBar
* force
* scatterplot

Building
--------

Run build.bat to download NuGet.exe and build the solution. You can find samples in the samples folder.

``` fsharp
#r "..\build\VegaHub.dll"

open System
open VegaHub
open VegaHub.Grammar
open VegaHub.Basics

let disposable = Vega.connect "http://localhost:8081" __SOURCE_DIRECTORY__

// Simulate real-time updates
let rand = Random(42)

let rec loop data iter = async {
    let data' = List.append data [ (data.Length, rand.Next(0, 100)) ]
    // Warning: mutation!   
    Basics.bar data' ((fun x -> fst x |> string), (fun x -> snd x |> float)) |> Vega.send
    do! Async.Sleep 100
    if iter = 0 then () else
    return! loop data' <| iter - 1
}

loop [] 25 |> Async.RunSynchronously

disposable.Dispose()
```
