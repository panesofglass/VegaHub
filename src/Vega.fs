(**
 * Vega Core
 *)

namespace VegaHub

open System
open System.Diagnostics
open Owin
open Microsoft.AspNet.SignalR
open Microsoft.Owin.Hosting
open ImpromptuInterface.FSharp
open VegaHub.Grammar

module internal WebApp =
    type ChartHub() =
        inherit Hub()

    let private attachHub (app: IAppBuilder) =
        app.MapSignalR()

    let private hostFiles (app: IAppBuilder) =
        app.UseStaticFiles __SOURCE_DIRECTORY__

    let launch (url: string) =
        let disposable = WebApp.Start(url, attachHub >> hostFiles >> ignore)
        disposable

module Vega =

    /// Launch the default web browser and connect SignalR using the specified url.
    let connect url =
        let disposable = WebApp.launch url
        Console.WriteLine("Running chart hub on " + url)
        // TODO: Use canopy?
        Process.Start(url + "/index.html") |> ignore
        disposable

    /// Send the spec to the Vega browser client via SignalR.
    let send (spec:string) : unit = 
        let hub = GlobalHost.ConnectionManager.GetHubContext<WebApp.ChartHub>()
        hub.Clients.All?parse spec