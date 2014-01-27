// Include dependencies
#I "../../bin"
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

// Reference canopy
#r "bin/Debug/WebDriver.dll"
#r "bin/Debug/WebDriver.Support.dll"
#r "bin/Debug/SizSelCsZzz.dll"
#r "bin/Debug/canopy.dll"

open System
open System.Diagnostics
open VegaHub
open VegaHub.Grammar
open VegaHub.Basics

open OpenQA.Selenium
open OpenQA.Selenium.Support
open canopy

// Launch the Vega Hub.
let requestUrl = "http://localhost:8081"
let disposable = Vega.connect(requestUrl, __SOURCE_DIRECTORY__)

// Launch the desired browser
start firefox
url (requestUrl + "/index.html")

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
