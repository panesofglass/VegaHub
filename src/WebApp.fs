module VegaHub.WebApp

open System
open Owin
open Microsoft.AspNet.SignalR
open Microsoft.Owin.Hosting
open Newtonsoft.Json

type ChartHub() =
    inherit Hub()

let private attachHub (app: IAppBuilder) =
    app.MapSignalR()

let private hostFiles (app: IAppBuilder) =
    app.UseStaticFiles __SOURCE_DIRECTORY__

let launch (url: string) =
    let disposable = WebApp.Start(url, attachHub >> hostFiles >> ignore)
    disposable
