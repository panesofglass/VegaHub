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

module internal WebApp =
    type ChartHub() =
        inherit Hub()

    let private attachHub (app: IAppBuilder) =
        app.MapSignalR()

    let private hostFiles filePath (app: IAppBuilder) =
        let options = StaticFileOptions(FileSystem = PhysicalFileSystem(filePath))
        app.UseStaticFiles(options)

    let internal launch (url: string) filePath =
        let disposable = WebApp.Start(url, attachHub >> hostFiles filePath >> ignore)
        disposable

module Vega =

    /// Launch the default web browser and connect SignalR using the specified url.
    [<CompiledName("Connect")>]
    let connect url filePath =
        let disposable = WebApp.launch url filePath
        Console.WriteLine("Running chart hub on " + url)
        // TODO: Use canopy?
        Process.Start(url + "/index.html") |> ignore
        disposable

    /// Send the spec to the Vega browser client via SignalR.
    [<CompiledName("Send")>]
    let send (spec:string) : unit = 
        let hub = GlobalHost.ConnectionManager.GetHubContext<WebApp.ChartHub>()
        hub.Clients.All?parse spec
