namespace VegaHub.Hosting

open System
open Owin
open Microsoft.AspNet.SignalR
open Microsoft.Owin.Hosting
open Nancy
open Nancy.Conventions

type ChartHub() =
    inherit Hub()

type Bootstrapper() =
    inherit DefaultNancyBootstrapper()
    override this.ConfigureConventions(nancyConventions: NancyConventions) =
        nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("web", @"web"))
        base.ConfigureConventions(nancyConventions)
    override this.RootPathProvider
        with get() =
            { new IRootPathProvider with
                member this.GetRootPath() = __SOURCE_DIRECTORY__
                member this.Equals(o) = this.Equals(o)
                member this.GetHashCode() = this.GetHashCode()
                member this.GetType() = this.GetType()
                member this.ToString() = this.ToString() }

type IndexModule() as x =
    inherit NancyModule()
    do x.Get.["/"] <- fun _ -> x.View.["index"] :> _

module Host =
    let private attachHub (app: IAppBuilder) =
        app.MapSignalR()

    let private hostFiles (app: IAppBuilder) =
        // TODO: Prefer default files.
        //app.UseDefaultFiles __SOURCE_DIRECTORY__
        // TODO: either pass in the path or detect it properly. FSI makes this hard.
        //app.UseStaticFiles __SOURCE_DIRECTORY__
        // Host with Nancy until the static file middleware is ready.
        app.UseNancy(Owin.NancyOptions(Bootstrapper = new Bootstrapper()))

    let launch (url: string) =
        let disposable = WebApp.Start(url, attachHub >> hostFiles >> ignore)
        disposable
