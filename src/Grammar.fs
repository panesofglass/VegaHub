(**
 * Vega Grammar
 *)

namespace VegaHub.Grammar

open System
open System.Collections.Generic

type Padding() =
    member val Top    : int = 0 with get,set
    member val Left   : int = 0 with get,set
    member val Bottom : int = 0 with get,set
    member val Right  : int = 0 with get,set

type Data<'T>() =
    member val Name   : string  = null with get,set
    member val Values : 'T[] = null with get,set

type ScaleDomain() =
    member val Data  : string = null with get,set
    member val Field : string = null with get,set

type Scale<'TRange>() =
    member val Name   : string = null with get,set
    member val Range  : 'TRange = Unchecked.defaultof<_> with get,set
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

type Spec<'TData, 'TRange>() =
    member val Name    : string = null with get,set
    member val Width   : int = 0 with get,set
    member val Height  : int = 0 with get,set
    member val Padding : Padding = Padding() with get,set
    member val Data    : Data<'TData>[] = null with get,set
    member val Scales  : Scale<'TRange>[] = null with get,set
    member val Axes    : Axis[] = null with get,set
    member val Marks   : Mark[] = null with get,set


(**
 * Data types
 *)

type Point() =
    member val X : int = 0 with get,set
    member val Y : int = 0 with get,set


(**
 * Serialization helpers
 *)

module internal Serialization =
    open Newtonsoft.Json

    let private settings =
        JsonSerializerSettings(ContractResolver = Serialization.CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore)

    let deserialize<'TData, 'TRange> spec =
        JsonConvert.DeserializeObject<Spec<'TData, 'TRange>>(spec, settings)

    let serialize spec =
        JsonConvert.SerializeObject(spec, settings)
