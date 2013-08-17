#I """..\packages"""
#r """Owin.1.0\lib\net40\Owin.dll"""
#r """Microsoft.Owin.2.0.0-rtw1-20808-529-dev\lib\net45\Microsoft.Owin.dll"""
#r """Microsoft.Owin.Diagnostics.2.0.0-rtw1-20808-529-dev\lib\net40\Microsoft.Owin.Diagnostics.dll"""
#r """Microsoft.Owin.FileSystems.0.24.0-pre-20808-531-dev\lib\net40\Microsoft.Owin.FileSystems.dll"""
#r """Microsoft.Owin.Hosting.2.0.0-rtw1-20808-529-dev\lib\net45\Microsoft.Owin.Hosting.dll"""
#r """Microsoft.Owin.Security.2.0.0-rtw1-20808-529-dev\lib\net45\Microsoft.Owin.Security.dll"""
#r """Microsoft.Owin.StaticFiles.0.24.0-pre-20808-531-dev\lib\net40\Microsoft.Owin.StaticFiles.dll"""
#r """Microsoft.Owin.Host.HttpListener.2.0.0-rtw1-20808-529-dev\lib\net45\Microsoft.Owin.Host.HttpListener.dll"""
#r """Newtonsoft.Json.5.0.6\lib\net45\Newtonsoft.Json.dll"""
#r """Microsoft.AspNet.SignalR.Core.2.0.0-rtm1-130808-b140\lib\net45\Microsoft.AspNet.SignalR.Core.dll"""
#r """ImpromptuInterface.6.2.2\lib\net40\ImpromptuInterface.dll"""
#r """ImpromptuInterface.FSharp.1.2.13\lib\net40\ImpromptuInterface.FSharp.dll"""
#load "WebApp.fs"
#load "Vega.fs"

open System
open VegaHub
open VegaHub.Vega

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
