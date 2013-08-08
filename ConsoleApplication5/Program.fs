open System
open Owin
open Microsoft.AspNet.SignalR
open Microsoft.Owin.Hosting
open ImpromptuInterface.FSharp

let addMessage message (target: obj) =
    target?addMessage(message)

let addData data (target: obj) =
    target?addData(data)

type ChartHub() =
    inherit Hub()
    member x.Send(message: string) : unit =
        x.Clients.All |> addMessage message

let attachHub (app: IAppBuilder) =
    let config = new HubConfiguration(EnableJSONP = true)
    app.MapSignalR(config) |> ignore

let sendMessage message (hub: IHubContext) =
    hub.Clients.All |> addMessage message

let sendData data (hub: IHubContext) =
    hub.Clients.All |> addData data

[<EntryPoint>]
let main argv = 
    let url = "http://localhost:8081"
    use __ = WebApp.Start(url, Action<_> attachHub)
    Console.WriteLine("Running chart hub on " + url)

    let hub = GlobalHost.ConnectionManager.GetHubContext<ChartHub>()

    Console.WriteLine("Press any key to send a message.")
    Console.ReadKey() |> ignore
    hub |> sendMessage "Hello, Mathias!"

    Console.WriteLine("Press any key to send data.")
    Console.ReadKey() |> ignore
    hub |> sendData [|1..10|]

    Console.WriteLine("Press any key to stop.")
    Console.ReadKey() |> ignore
    0 // return an integer exit code
