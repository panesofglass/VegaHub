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
open ImpromptuInterface.FSharp

let addMessage message (target: obj) : unit = target?addMessage(message)
let addData data (target: obj) : unit = target?addData(data)

type ChartHub() =
    inherit Hub()
    member x.Send(message: string) : unit =
        x.Clients.All |> addMessage message

let attachHub (app: IAppBuilder) =
    let config = new HubConfiguration(EnableJSONP = true)
    app.MapSignalR(config) |> ignore
    //app.UseDefaultFiles("""M:\Code\ConsoleApplication5\ConsoleApplication5""") |> ignore
    app.UseStaticFiles("""M:\Code\ConsoleApplication5\ConsoleApplication5""") |> ignore

let sendMessage message (hub: IHubContext) =
    hub.Clients.All |> addMessage message

let sendData data (hub: IHubContext) =
    hub.Clients.All |> addData data

let launch (url: string) =
    use __ = WebApp.Start(url, Action<_> attachHub)
    Console.WriteLine("Running chart hub on " + url)
    // TODO: Use canopy?
    Process.Start(url + "/index.html") |> ignore
    GlobalHost.ConnectionManager.GetHubContext<ChartHub>()

[<EntryPoint>]
let main argv = 
    let hub = launch "http://localhost:8081"

    Console.WriteLine("Press any key to send a message.")
    Console.ReadKey() |> ignore
    hub |> sendMessage "Hello, Mathias!" |> ignore

    Console.WriteLine("Press any key to send data.")
    Console.ReadKey() |> ignore
    hub |> sendData [|1..10|] |> ignore

    Console.WriteLine("Press any key to stop.")
    Console.ReadKey() |> ignore
    0 // return an integer exit code
