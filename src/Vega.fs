module VegaHub.Vega

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.AspNet.SignalR
open Newtonsoft.Json
open ImpromptuInterface.FSharp
open VegaHub

(**
 * Vega Grammar
 *)

[<CLIMutable>]
type Padding =
    { Top    : int
      Left   : int
      Bottom : int
      Right  : int }

[<CLIMutable>]
type Point = { x: int; y: int }

[<CLIMutable>]
type Data = { Name : string; Values : Point[] }

[<CLIMutable>]
type ScaleDomain = { Data : string; Field : string }

[<CLIMutable>]
type Scale =
    { Name   : string
      Nice   : bool
      Range  : string 
      Type   : string
      Domain : ScaleDomain }

[<CLIMutable>]
type Axis = { Type : string; Scale : string }

[<CLIMutable>]
type MarkFrom = { Data : string }

[<CLIMutable>]
type Mark =
    { Type       : string
      From       : MarkFrom
      Properties : IDictionary<string, obj> } // TODO: Create a typed wrapper with property accessors

[<CLIMutable>]
type Spec =
    { Width   : int
      Height  : int
      Padding : Padding
      Data    : Data[]
      Scales  : Scale[]
      Axes    : Axis[]
      Marks   : Mark[] }

(**
 * Vega Templates
 *)

module Templates =
    let bar =
        { Width = 400
          Height = 200
          Padding = { Top = 10; Left = 30; Bottom = 30; Right = 10 }
          Data = [| { Name = "table"; Values = [||] } |]
          Scales =
            [|
              {
                Name = "x"
                Type = "ordinal"
                Range = "width"
                Nice = false
                Domain = { Data = "table"; Field = "data.x" }
              }
              {
                Name = "y"
                Type = null
                Range = "height"
                Nice = true
                Domain = { Data = "table"; Field = "data.y" }
              }
            |]
          Axes = [| { Type = "x"; Scale = "x" }; { Type = "y"; Scale = "y" } |]
          Marks =
            [|
              {
                Type = "rect"
                From = { Data = "table" }
                Properties =
                  [|
                    ("enter",
                      [|
                        ("x", dict [|("scale", box "x"); ("field", box "data.x")|])
                        ("width", dict [|("scale", box "x"); ("band", box true); ("offset", box -1)|])
                        ("y", dict [|("scale", box "y"); ("field", box "data.y")|])
                        ("y2", dict [|("scale", box "y"); ("value", box 0)|])
                      |] |> dict |> box)
                    ("update", [|("fill", dict [|("value", box "steelblue")|])|] |> dict |> box)
                    ("hover", [|("fill", dict [|("value", box "red")|])|] |> dict |> box)
                  |] |> dict
              }
            |]
        }

(**
 * Vega Core
 *)

/// Launch the default web browser and connect SignalR using the specified url.
let connect url =
    let disposable = WebApp.launch url
    Console.WriteLine("Running chart hub on " + url)
    // TODO: Use canopy?
    Process.Start(url + "/index.html") |> ignore
    disposable

let private settings =
    JsonSerializerSettings(ContractResolver = Serialization.CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore)

let serialize spec =
    JsonConvert.SerializeObject(spec, Formatting.Indented, settings)

/// Send the spec to the Vega browser client via SignalR.
let send (spec: Spec) : unit = 
    let hub = GlobalHost.ConnectionManager.GetHubContext<WebApp.ChartHub>()
    hub.Clients.All?parse (serialize spec)
