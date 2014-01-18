// Include dependencies
#I """..\packages"""
#r """Owin.1.0\lib\net40\Owin.dll"""
#r """Microsoft.Owin.2.1.0-rc1\lib\net45\Microsoft.Owin.dll"""
#r """Microsoft.Owin.FileSystems.2.1.0-rc1\lib\net40\Microsoft.Owin.FileSystems.dll"""
#r """Microsoft.Owin.Hosting.2.1.0-rc1\lib\net45\Microsoft.Owin.Hosting.dll"""
#r """Microsoft.Owin.Security.2.1.0-rc1\lib\net45\Microsoft.Owin.Security.dll"""
#r """Microsoft.Owin.StaticFiles.2.1.0-rc1\lib\net40\Microsoft.Owin.StaticFiles.dll"""
#r """Microsoft.Owin.Host.HttpListener.2.1.0-rc1\lib\net45\Microsoft.Owin.Host.HttpListener.dll"""
#r """Newtonsoft.Json.5.0.6\lib\net45\Newtonsoft.Json.dll"""
#r """Microsoft.AspNet.SignalR.Core.2.0.1\lib\net45\Microsoft.AspNet.SignalR.Core.dll"""
#r """ImpromptuInterface.6.2.2\lib\net40\ImpromptuInterface.dll"""
#r """ImpromptuInterface.FSharp.1.2.13\lib\net40\ImpromptuInterface.FSharp.dll"""

// Reference VegaHub
#r """..\src\bin\Debug\VegaHub.dll"""

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
