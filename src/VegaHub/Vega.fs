(**
 * Vega Core
 *)

namespace VegaHub

open System
open System.Diagnostics
open Owin
open Microsoft.AspNet.SignalR
open Microsoft.Owin.FileSystems
open Microsoft.Owin.Hosting
open Microsoft.Owin.StaticFiles
open ImpromptuInterface.FSharp
open VegaHub.Grammar

/// SignalR hub over which to communicate vega messages.
type ChartHub() =
    inherit Hub()

/// Module to used to connect an F# Interactive session with the SignalR hub and send messages.
module Vega =
    let private attachHub (app: IAppBuilder) =
        app.MapSignalR()

    let private hostFiles filePath (app: IAppBuilder) =
        let options = StaticFileOptions(FileSystem = PhysicalFileSystem(filePath))
        app.UseStaticFiles(options)

    /// Launch the default web browser and connect SignalR using the specified url.
    [<CompiledName("Connect")>]
    let connect(url: string, filePath) =
        let disposable = WebApp.Start(url, attachHub >> hostFiles filePath >> ignore)
        printfn "Running chart hub on %s" url
        disposable

    /// Send the spec to the Vega browser client via SignalR.
    [<CompiledName("Send")>]
    let send(spec:string) : unit = 
        let hub = GlobalHost.ConnectionManager.GetHubContext<ChartHub>()
        hub.Clients.All?parse spec
