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
VegaHub
===================

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The VegaHub library can be <a href="https://nuget.org/packages/VegaHub">installed from NuGet</a>:
      <pre>PM> Install-Package VegaHub -Pre</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

<img src="img/logo.png" alt="F# Project" style="float:right;width:150px;margin:10px" />

Example
-------

This example demonstrates animating a bar chart using random values.

*)

let requestUrl = "http://localhost:8081"
let disposable = Vega.connect(requestUrl, __SOURCE_DIRECTORY__)
System.Diagnostics.Process.Start(requestUrl + "/index.html")

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

(**
Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include a tutorials automatically generated from `*.fsx` files in [the content folder][content]. 
The API reference is automatically generated from Markdown comments in the library implementation.

 * [Getting Started](tutorial.html) contains a further explanation of this library.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/panesofglass/VegaHub/tree/master/docs/content
  [gh]: https://github.com/panesofglass/VegaHub
  [issues]: https://github.com/panesofglass/VegaHub/issues
  [readme]: https://github.com/panesofglass/VegaHub/blob/master/README.md
  [license]: https://github.com/panesofglass/VegaHub/blob/master/LICENSE.txt
*)
