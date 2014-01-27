// NOTE: You should build the current project first so that the required assemblies are found in bin/Debug.

// Include dependencies
#I "bin/Debug"
#r "Owin.dll"
#r "Microsoft.Owin.dll"
#r "Microsoft.Owin.FileSystems.dll"
#r "Microsoft.Owin.Hosting.dll"
#r "Microsoft.Owin.Security.dll"
#r "Microsoft.Owin.StaticFiles.dll"
#r "Microsoft.Owin.Host.HttpListener.dll"
#r "Newtonsoft.Json.dll"
#r "Microsoft.AspNet.SignalR.Core.dll"
#r "ImpromptuInterface.dll"
#r "ImpromptuInterface.FSharp.dll"

// Reference VegaHub
#r "VegaHub.dll"

open System
open VegaHub
open VegaHub.Grammar
open VegaHub.Basics

let requestUrl = "http://localhost:8081"
let disposable = Vega.Connect(requestUrl, __SOURCE_DIRECTORY__)
System.Diagnostics.Process.Start(requestUrl + "/index.html")

// Simulate real-time updates
let rand = Random(42)

let rec loop data iter = async {
    let data' = List.append data [ (data.Length, rand.Next(0, 100)) ]
    // Warning: mutation!   
    Basics.bar data' ((fun x -> fst x |> string), (fun x -> snd x |> float)) |> Vega.Send
    do! Async.Sleep 100
    if iter = 0 then () else
    return! loop data' <| iter - 1
}

loop [] 25 |> Async.RunSynchronously

disposable.Dispose()
