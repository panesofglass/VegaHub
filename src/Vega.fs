namespace VegaHub

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.AspNet.SignalR
open Newtonsoft.Json
open ImpromptuInterface.FSharp
open VegaHub.Hosting

(**
 * Vega Grammar
 *)

type Padding() =
    member val Top    : int = 0 with get,set
    member val Left   : int = 0 with get,set
    member val Bottom : int = 0 with get,set
    member val Right  : int = 0 with get,set

type Point() =
    member val X : int = 0 with get,set
    member val Y : int = 0 with get,set

type Data() =
    member val Name   : string  = null with get,set
    member val Values : Point[] = null with get,set

type ScaleDomain() =
    member val Data  : string = null with get,set
    member val Field : string = null with get,set

type Scale() =
    member val Name   : string = null with get,set
    member val Range  : string = null with get,set
    member val Domain : ScaleDomain = ScaleDomain() with get,set
    member val Nice   : bool = false with get,set
    member val Type   : string = null with get,set
    member val Zero   : bool = true with get,set

type Axis() =
    member val Type  : string = null with get,set
    member val Scale : string = null with get,set
    member val Ticks : int = 0 with get,set

type MarkFrom() =
    member val Data : string = null with get,set

type Mark() =
    member val Type : string = null with get,set
    member val From : MarkFrom = MarkFrom() with get,set
    member val Properties : IDictionary<string, obj> = null with get,set // TODO: Create a typed wrapper with property accessors

type Spec() =
    member val Name    : string = null with get,set
    member val Width   : int = 0 with get,set
    member val Height  : int = 0 with get,set
    member val Padding : Padding = Padding() with get,set
    member val Data    : Data[] = null with get,set
    member val Scales  : Scale[] = null with get,set
    member val Axes    : Axis[] = null with get,set
    member val Marks   : Mark[] = null with get,set


(**
 * Serializer helpers
 *)

module internal Serialization =
    let private settings =
        JsonSerializerSettings(ContractResolver = Serialization.CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore)

    let deserialize spec =
        JsonConvert.DeserializeObject<Spec>(spec, settings)

    let serialize spec =
        JsonConvert.SerializeObject(spec, settings)


(**
 * Vega Templates
 *)

module Templates =
//    let arc =
//        Serialization.deserialize """{
//  "name": "arc",
//  "width": 400,
//  "height": 400,
//  "data": [
//    {
//      "name": "table",
//      "values": [12, 23, 47, 6, 52, 19],
//      "transform": [
//        {"type": "pie", "value": "data"}
//      ]
//    }
//  ],
//  "scales": [
//    {
//      "name": "r",
//      "type": "sqrt",
//      "domain": {"data": "table", "field": "data"},
//      "range": [20, 100]
//    }
//  ],
//  "marks": [
//    {
//      "type": "arc",
//      "from": {"data": "table"},
//      "properties": {
//        "enter": {
//          "x": {"group": "width", "mult": 0.5},
//          "y": {"group": "height", "mult": 0.5},
//          "startAngle": {"field": "startAngle"},
//          "endAngle": {"field": "endAngle"},
//          "innerRadius": {"value": 20},
//          "outerRadius": {"scale": "r"},
//          "stroke": {"value": "#fff"}
//        },
//        "update": {
//          "fill": {"value": "#ccc"}
//        },
//        "hover": {
//          "fill": {"value": "pink"}
//        }
//      }
//    }
//  ]
//}"""

    let area =
        Serialization.deserialize """{
  "width": 500,
  "height": 200,
  "padding": {"top": 10, "left": 30, "bottom": 30, "right": 10},
  "data": [
    {
      "name": "table",
      "values": [
        {"x": 1,  "y": 28}, {"x": 2,  "y": 55},
        {"x": 3,  "y": 43}, {"x": 4,  "y": 91},
        {"x": 5,  "y": 81}, {"x": 6,  "y": 53},
        {"x": 7,  "y": 19}, {"x": 8,  "y": 87},
        {"x": 9,  "y": 52}, {"x": 10, "y": 48},
        {"x": 11, "y": 24}, {"x": 12, "y": 49},
        {"x": 13, "y": 87}, {"x": 14, "y": 66},
        {"x": 15, "y": 17}, {"x": 16, "y": 27},
        {"x": 17, "y": 68}, {"x": 18, "y": 16},
        {"x": 19, "y": 49}, {"x": 20, "y": 15}
      ]
    }
  ],
  "scales": [
    {
      "name": "x",
      "type": "linear",
      "range": "width",
      "zero": false,
      "domain": {"data": "table", "field": "data.x"}
    },
    {
      "name": "y",
      "type": "linear",
      "range": "height",
      "nice": true,
      "domain": {"data": "table", "field": "data.y"}
    }
  ],
  "axes": [
    {"type": "x", "scale": "x", "ticks": 20},
    {"type": "y", "scale": "y"}
  ],
  "marks": [
    {
      "type": "area",
      "from": {"data": "table"},
      "properties": {
        "enter": {
          "interpolate": {"value": "monotone"},
          "x": {"scale": "x", "field": "data.x"},
          "y": {"scale": "y", "field": "data.y"},
          "y2": {"scale": "y", "value": 0},
          "fill": {"value": "steelblue"}
        },
        "update": {
          "fillOpacity": {"value": 1}
        },
        "hover": {
          "fillOpacity": {"value": 0.5}
        }
      }
    }
  ]
}"""
        
    let bar =
        Serialization.deserialize """{
  "width": 400,
  "height": 200,
  "padding": {"top": 10, "left": 30, "bottom": 30, "right": 10},
  "data": [
    {
      "name": "table",
      "values": [
        {"x": 1,  "y": 28}, {"x": 2,  "y": 55},
        {"x": 3,  "y": 43}, {"x": 4,  "y": 91},
        {"x": 5,  "y": 81}, {"x": 6,  "y": 53},
        {"x": 7,  "y": 19}, {"x": 8,  "y": 87},
        {"x": 9,  "y": 52}, {"x": 10, "y": 48},
        {"x": 11, "y": 24}, {"x": 12, "y": 49},
        {"x": 13, "y": 87}, {"x": 14, "y": 66},
        {"x": 15, "y": 17}, {"x": 16, "y": 27},
        {"x": 17, "y": 68}, {"x": 18, "y": 16},
        {"x": 19, "y": 49}, {"x": 20, "y": 15}
      ]
    }
  ],
  "scales": [
    {
      "name": "x",
      "type": "ordinal",
      "range": "width",
      "domain": {"data": "table", "field": "data.x"}
    },
    {
      "name": "y",
      "range": "height",
      "nice": true,
      "domain": {"data": "table", "field": "data.y"}
    }
  ],
  "axes": [
    {"type": "x", "scale": "x"},
    {"type": "y", "scale": "y"}
  ],
  "marks": [
    {
      "type": "rect",
      "from": {"data": "table"},
      "properties": {
        "enter": {
          "x": {"scale": "x", "field": "data.x"},
          "width": {"scale": "x", "band": true, "offset": -1},
          "y": {"scale": "y", "field": "data.y"},
          "y2": {"scale": "y", "value": 0}
        },
        "update": {
          "fill": {"value": "steelblue"}
        },
        "hover": {
          "fill": {"value": "red"}
        }
      }
    }
  ]
}"""

(**
 * Vega Core
 *)

module Vega =
    /// Launch the default web browser and connect SignalR using the specified url.
    let connect url =
        let disposable = Host.launch url
        Console.WriteLine("Running chart hub on " + url)
        // TODO: Use canopy?
        Process.Start(url) |> ignore
        disposable

    /// Send the spec to the Vega browser client via SignalR.
    let send (spec: Spec) : unit = 
        let hub = GlobalHost.ConnectionManager.GetHubContext<ChartHub>()
        hub.Clients.All?parse (Serialization.serialize spec)
