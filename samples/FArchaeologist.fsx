// Include dependencies
#I """..\packages"""
#r """Owin.1.0\lib\net40\Owin.dll"""
#r """Microsoft.Owin.2.1.0-rc1\lib\net45\Microsoft.Owin.dll"""
#r """Microsoft.Owin.FileSystems.2.1.0-rc1\lib\net40\Microsoft.Owin.FileSystems.dll"""
#r """Microsoft.Owin.Hosting.2.1.0-rc1\lib\net45\Microsoft.Owin.Hosting.dll"""
#r """Microsoft.Owin.Security.2.1.0-rc1\lib\net45\Microsoft.Owin.Security.dll"""
#r """Microsoft.Owin.StaticFiles.2.1.0-rc1\lib\net40\Microsoft.Owin.StaticFiles.dll"""
#r """Microsoft.Owin.Host.HttpListener.2.1.0-rc1\lib\net45\Microsoft.Owin.Host.HttpListener.dll"""
#r """Newtonsoft.Json.5.0.6\lib\net45\Newtonsoft.Json.dll"""
#r """Microsoft.AspNet.SignalR.Core.2.0.1\lib\net45\Microsoft.AspNet.SignalR.Core.dll"""
#r """ImpromptuInterface.6.2.2\lib\net40\ImpromptuInterface.dll"""
#r """ImpromptuInterface.FSharp.1.2.13\lib\net40\ImpromptuInterface.FSharp.dll"""
#r """FSharp.Data.1.1.10\lib\net40\FSharp.Data.dll"""

// Reference VegaHub
#r """..\src\bin\Debug\VegaHub.dll"""

// Reference Twitter.API
#r """references\Twitter.API.dll"""
#load "references/GuiExtensions.fs"

open System
open System.Threading
open System.Windows.Forms
open System.Collections.Generic

open FSharp.Control
open FSharp.WebBrowser
open FSharp.TwitterAPI

open VegaHub
open VegaHub.Grammar
open VegaHub.Basics


// ----------------------------------------------------------------------------
// Retrieve discussions for the specified tag
// ----------------------------------------------------------------------------

type Node = { Name: string; Group: int; Id: int }

let run term count context =
    let buzz = Twitter.Search.Tweets(context, term, count=count)

    // Collect mentions by user
    let discussions =
        buzz.Statuses
        |> Array.map (fun status ->
            status.User.ScreenName, status.Entities.UserMentions |> Array.map (fun x -> x.ScreenName))


    // ----------------------------------------------------------------------------
    // Generate nodes for a force graph
    // ----------------------------------------------------------------------------

    let createNode index (name, count: int) =
        { Name = name
          Group = count/10 + 1
          Id = index }

    let nodes =
        discussions
        |> Array.map snd
        |> Array.collect id
        |> Seq.countBy id
        |> Seq.mapi createNode
        |> Seq.toList


    // ----------------------------------------------------------------------------
    // Generate the link relations for all nodes
    // ----------------------------------------------------------------------------

    let isMentioned (sender, _) =
        nodes
        |> List.exists (fun node -> node.Name = sender)

    let findName name =
        nodes
        |> List.tryFind (fun d -> d.Name = name)

    let createLink (sender, mentions) =
        mentions
        |> Array.choose (fun mention ->
            findName sender |> Option.bind (fun source ->
            findName mention |> Option.bind (fun target ->
            Some(source.Id, target.Id, target.Group) )))

    // Retrieve the links, filtering the results to only those with a mention.
    let links =
        discussions
        |> Array.filter isMentioned
        |> Array.map createLink
        |> Array.collect id
        |> Seq.distinct
        |> Seq.toList

    nodes, links

// ----------------------------------------------------------------------------
// Create user interface and connect to Twitter
// ----------------------------------------------------------------------------

let frm = new Form(TopMost = true, Visible = true, Width = 500, Height = 400)
let btn = new Button(Text = "Pause", Dock = DockStyle.Top)
let web = new WebBrowser(Dock = DockStyle.Fill)
frm.Controls.Add(web)
frm.Controls.Add(btn)

let key = "Your Twitter API key"
let secret = "Your Twitter API secret"
let connector = Twitter.Authenticate(key, secret, web.Navigate)

// NOTE: Run all code up to this point. A window should appear. You can then
// login to twitter and you'll get a pin code that you need to copy and
// paste as an argument to the 'Connect' method below:
let twitter = connector.Connect("Your pin code")

// Login: 'fsharpd'
// Password: 'fsharp123'


// ----------------------------------------------------------------------------
// Launch VegaHub and pump the nodes into the force chart
// ----------------------------------------------------------------------------

let disposable = Vega.connect "http://localhost:8081" __SOURCE_DIRECTORY__

let shouldRun = ref true
while !shouldRun do
    let nodes, links = run "MVPSummit" 100 twitter
    Basics.force nodes (fun (n: Node) -> n.Name)
                 links ((fun (s,_,_) -> float s), (fun (_,t,_) -> float t), (fun (_,_,v) -> float v))
                 (70., -100., 1000)
                 |> Vega.send
    System.Threading.Thread.Sleep 60000

shouldRun := false

disposable.Dispose()
