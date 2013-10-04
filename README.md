VegaHub
=======

A SignalR Hub Utility for data scientists to push Vega charts from F# Interactive

Current Status
--------------

Proof-of-Concept

### Available charts

* [VegaHub.Templates.area](https://github.com/panesofglass/VegaHub/blob/master/src/Vega.fs#L132)
* [VegaHub.Templates.bar](https://github.com/panesofglass/VegaHub/blob/master/src/Vega.fs#L197)

Building
--------

VegaHub relies on several packages in the ASP.NET Web Stack nightly builds. You will need to add a reference to the MyGet feed in order to successfully build and run the script.

Add this to your NuGet package sources by going to Tools -> Library Package Manager -> Package Manager Settings and creating a new package source for http://www.myget.org/f/aspnetwebstacknightlyrelease/

Once you add this package source, you should build from the command line using build.bat in order to download NuGet.exe.

``` fsharp
#load "WebApp.fs"
#load "Vega.fs"

open System
open VegaHub

let disposable = Vega.connect "http://localhost:8081"

// Simulate real-time updates
let rand = Random(42)
//let spec = Templates.bar
let spec = Templates.area
let rec loop data iter = async {
    let data' = Array.append data [| Point(X = data.Length, Y = rand.Next(0, 100)) |]
    // Warning: mutation!
    spec.Data <- [| Data(Name = "table", Values = data') |]
    Vega.send spec
    do! Async.Sleep 100
    if iter = 0 then () else
    return! loop data' <| iter - 1
}

loop [||] 25 |> Async.RunSynchronously

disposable.Dispose()
```
