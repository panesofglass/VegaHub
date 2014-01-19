(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I """..\..\bin"""
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

(**
Getting Started
========================

In this tutorial, we'll explore the basics of generating charts from F# Interactive with VegaHub.

To begin, create a new F# class library or console application. Remove the Script.fsx file, if one was created.
Next, use NuGet to install the VegaHub package.

    Install-Package VegaHub -Pre

NuGet will install VegaHub along with its required dependencies and place a number of files in your project, including:

* index.html
* Script.fsx
* Scripts/app.js
* Scripts/d3.geo.projection.min.js
* Scripts/d3.layout.cloud.js
* Scripts/d3.v3.min.js
* Scripts/topojson.js
* Scripts/vega.js

These files provide you with a bare-bones HTML page with the necessary JavaScript libraries to run visualizations.
The Script.fsx is similar to the sample in the [VegaHub.Samples](https://github.com/panesofglass/VegaHub/tree/master/samples/VegaHub.Samples) project.
This simple sample produces a bar chart using random values.
A loop adds a new value and redraws the chart after a 100 ms sleep using both the old and new values.
`Basics.bar` represents a Vega spec and accepts the data of tuples and two functions,
one to retrieve the name of the column and another to retrieve the value for that column.
The `Vega.send` function accepts a Vega spec and pushes the spec to the browser using the SignalR `ChartHub`.
*)

// Connect to the Vega hub.
let requestUrl = "http://localhost:8081"
let disposable = Vega.connect(requestUrl, __SOURCE_DIRECTORY__)

// Launch the browser using the system's default browser.
// If you would like to specify a browser, try [canopy](http://lefthandedgoat.github.io/canopy/)
// Note that you need to call this after you run `Vega.connect`.
System.Diagnostics.Process.Start(requestUrl + "/index.html")

// Simulate real-time updates.
let rand = Random(42)

// Define a loop function to generate and send the bar chart data.
let rec loop data iter = async {
    // Add a new, random value to the list on each iteration.
    let data' = List.append data [ (data.Length, rand.Next(0, 100)) ]
    // Create the bar chart spec.
    Basics.bar data' ((fun x -> fst x |> string), (fun x -> snd x |> float))
    // Send the spec to the browser.
    |> Vega.send
    // Delay for 100 ms.
    do! Async.Sleep 100
    // Stop when the counter reaches 0.
    if iter = 0 then () else
    // Keep running the loop, decrementing the counter on each iteration by 1.
    return! loop data' <| iter - 1
}

// Run the loop.
loop [] 25 |> Async.RunSynchronously

// Clean up.
disposable.Dispose()

(**
TODO: Describe other samples
*)