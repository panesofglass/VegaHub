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

/// Module to used to connect an F# Interactive session with the SignalR hub and send messages.
[<AbstractClass>]
type Vega =

    /// <summary>
    /// Launch the default web browser and connect SignalR using the specified url.
    /// </summary>
    /// <param name="url">The root url at which to run the OWIN application.</param>
    /// <param name="filePath">The file system path to HTML, JavaScript, and CSS files.</param>
    /// <param name="page">The name of the HTML file to launch, or "index.html" by default.</param>
    /// <returns>
    /// The <see cref="System.IDisposable" /> instance used to stop running the application and clean up resources.
    /// </returns>
    static member Connect(url, filePath, ?page) =
        let page = defaultArg page "/index.html"
        let disposable = WebApp.launch url filePath
        printfn "Running chart hub on %s" url
        printfn "Using %s from %s" page filePath
        // TODO: Use canopy?
        Process.Start(url + page) |> ignore
        disposable

    /// Send the spec to the Vega browser client via SignalR.
    static member Send(spec:string) : unit = 
        let hub = GlobalHost.ConnectionManager.GetHubContext<WebApp.ChartHub>()
        hub.Clients.All?parse spec
