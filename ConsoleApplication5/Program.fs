#if INTERACTIVE
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
#endif

open System
open System.Diagnostics
open Owin
open Microsoft.AspNet.SignalR
open Microsoft.Owin.StaticFiles
open Microsoft.Owin.Hosting
open Newtonsoft.Json
open ImpromptuInterface.FSharp

module WebApp =
    type ChartHub() =
        inherit Hub()

    let private attachHub (app: IAppBuilder) =
        let config = new HubConfiguration(EnableJSONP = true)
        app.MapSignalR(config) |> ignore
        // TODO: Prefer default files.
        //app.UseDefaultFiles("""M:\Code\ConsoleApplication5\ConsoleApplication5""") |> ignore
        // TODO: either pass in the path or detect it properly. FSI makes this hard.
        app.UseStaticFiles("""M:\Code\ConsoleApplication5\ConsoleApplication5""") |> ignore

    let launch (url: string) =
        let disposable = WebApp.Start(url, Action<_> attachHub)
        Console.WriteLine("Running chart hub on " + url)
        // TODO: Use canopy?
        Process.Start(url + "/index.html") |> ignore
        disposable

module Vega =
    let private sendSpec (spec: string) (hub: IHubContext) : unit =
        hub.Clients.All?parse spec 

    let private toBarJSON data =
        JsonConvert.SerializeObject(data)
        |> sprintf """{
  "width": 400,
  "height": 200,
  "padding": {"top": 10, "left": 30, "bottom": 30, "right": 10},
  "data": [
    {
      "name": "table",
      "values": %s
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

    let bar data = 
        GlobalHost.ConnectionManager.GetHubContext<WebApp.ChartHub>()
        |> sendSpec (data |> toBarJSON)

[<CLIMutable>]
type Point = { x: int; y: int }

[<EntryPoint>]
let main argv = 
    let disposable = WebApp.launch "http://localhost:8081"

    Console.WriteLine("Press any key to send a spec.")
    Console.ReadKey() |> ignore

    let data = [|
        {x = 1;  y = 28}
        {x = 2;  y = 55}
        {x = 3;  y = 43}
        {x = 4;  y = 91}
        {x = 5;  y = 81}
        {x = 6;  y = 53}
        {x = 7;  y = 19}
        {x = 8;  y = 87}
        {x = 9;  y = 52}
        {x = 10; y = 48}
        {x = 11; y = 24}
        {x = 12; y = 49}
        {x = 13; y = 87}
        {x = 14; y = 66}
        {x = 15; y = 17}
        {x = 16; y = 27}
        {x = 17; y = 68}
        {x = 18; y = 16}
        {x = 19; y = 49}
        {x = 20; y = 15}
    |]

    // Simulate real-time updates
    let rand = Random(42)
    let rec loop data iter = async {
        let data' =
            Array.append data [| {x = data.Length; y = rand.Next(0, 100)} |]

        data' |> Vega.bar

        do! Async.Sleep 100

        if iter = 0 then () else
        return! loop data' <| iter - 1
    }
    
    loop data 25 |> Async.RunSynchronously

    Console.WriteLine("Press any key to stop.")
    Console.ReadKey() |> ignore
    disposable.Dispose()
    0 // return an integer exit code
