#if BOOT
open Fake
module FB = Fake.Boot
FB.Prepare {
    FB.Config.Default __SOURCE_DIRECTORY__ with
        NuGetDependencies =
            let (!!) x = FB.NuGetDependency.Create x
            [
                !!"FAKE"
                !!"NuGet.Build"
                !!"NuGet.Core"
                !!"NUnit.Runners"
            ]
}
#endif

#load ".build/boot.fsx"

open System.IO
open Fake 
open Fake.AssemblyInfoFile
open Fake.MSBuild

(* properties *)
let projectName = "VegaHub"
let version = if isLocalBuild then "2.0." + System.DateTime.UtcNow.ToString("yMMdd") else buildVersion
let projectSummary = "A self-hosted SignalR hub utility for pushing Vega specs to a browser window from F# Interactive."
let projectDescription = "A self-hosted SignalR hub utility for pushing Vega specs to a browser window from F# Interactive."
let authors = ["Ryan Riley"; "Mathias Brandewinder"]
let mail = "ryan.riley@panesofglass.org"
let homepage = "https://github.com/panesofglass/VegaHub"

(* Directories *)
let buildDir = __SOURCE_DIRECTORY__ @@ "build"
let deployDir = __SOURCE_DIRECTORY__ @@ "deploy"
let packagesDir = __SOURCE_DIRECTORY__ @@ "packages"
let sources = __SOURCE_DIRECTORY__ @@ "src"

(* tools *)
let nugetPath = ".nuget/NuGet.exe"

(* files *)
let appReferences =
    !+ "src/**/*.fsproj" 
        |> Scan

(* Targets *)
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "BuildApp" (fun _ -> 
    if not isLocalBuild then
        [ Attribute.Version(version)
          Attribute.Title(projectName)
          Attribute.Description(projectDescription)
          Attribute.Guid("50151d7a-5e87-4535-8150-2f63a68d0040")
        ]
        |> CreateFSharpAssemblyInfo "src/AssemblyInfo.fs"

    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "Deploy" DoNothing
Target "Default" DoNothing

(* Build Order *)
"Clean"
    ==> "BuildApp"
    ==> "Deploy"

"Default" <== ["Deploy"]

// start build
RunTargetOrDefault "Default"

