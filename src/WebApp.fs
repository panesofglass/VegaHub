module VegaHub.WebApp

open System
open System.Diagnostics
open Owin
open Microsoft.AspNet.SignalR
open Microsoft.Owin.Hosting
open Newtonsoft.Json

type ChartHub() =
    inherit Hub()

let private attachHub (app: IAppBuilder) =
    let config = new HubConfiguration(EnableJSONP = true)
    app.MapSignalR(config)

let private hostFiles (app: IAppBuilder) =
    // TODO: Prefer default files.
    //app.UseDefaultFiles __SOURCE_DIRECTORY__
    // TODO: either pass in the path or detect it properly. FSI makes this hard.
    app.UseStaticFiles __SOURCE_DIRECTORY__

let launch (url: string) =
    let disposable = WebApp.Start(url, attachHub >> hostFiles >> ignore)
    Console.WriteLine("Running chart hub on " + url)
    // TODO: Use canopy?
    Process.Start(url + "/index.html") |> ignore
    disposable
