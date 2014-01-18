// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open System
open System.IO
open Fake 
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.MSBuild
open Fake.ReleaseNotesHelper

(* properties *)
let projectName = "VegaHub"
let version = if isLocalBuild then "2.0." + System.DateTime.UtcNow.ToString("yMMdd") else buildVersion
let projectSummary = "A self-hosted SignalR hub utility for pushing Vega specs to a browser window from F# Interactive."
let projectDescription = "A self-hosted SignalR hub utility for pushing Vega specs to a browser window from F# Interactive."
let authors = ["Ryan Riley"; "Mathias Brandewinder"]
let mail = "ryan.riley@panesofglass.org"
let homepage = "https://github.com/panesofglass/VegaHub"

(* Directories *)
let buildDir = __SOURCE_DIRECTORY__ @@ "bin"
let packagesDir = __SOURCE_DIRECTORY__ @@ "packages"
let sources = __SOURCE_DIRECTORY__ @@ "src"

(* tools *)
let nugetPath = ".nuget/NuGet.exe"

let RestorePackageParamF = 
  fun (p: RestorePackageParams) ->
    { p with
        ToolPath = nugetPath
        Sources = []
        TimeOut = System.TimeSpan.FromMinutes 5.
        OutputPath = "./packages" 
    } :Fake.RestorePackageHelper.RestorePackageParams

let RestorePackages2() = 
  !! "./**/packages.config"
  |> Seq.iter (RestorePackage RestorePackageParamF)
RestorePackages2()

(* files *)
let appReferences = !! "src/**/*.fsproj" 

(* Targets *)
Target "Clean" (fun _ ->
    CleanDirs [buildDir]
)

Target "BuildApp" (fun _ -> 
    if not isLocalBuild then
        [ Attribute.Version(version)
          Attribute.Title(projectName)
          Attribute.Description(projectDescription)
          Attribute.Guid("50151d7a-5e87-4535-8150-2f63a68d0040")
        ]
        |> CreateFSharpAssemblyInfo "src/VegaHub/AssemblyInfo.fs"

    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "CopyLicense" (fun _ ->
    [ "LICENSE.txt" ] |> CopyTo buildDir
)

Target "Deploy" DoNothing
Target "Default" DoNothing

(* Build Order *)
"Clean"
    ==> "BuildApp" <=> "CopyLicense"
    ==> "Deploy"

"Default" <== ["Deploy"]

// start build
RunTargetOrDefault "Default"
