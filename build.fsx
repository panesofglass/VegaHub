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
                !!"FSharp.Formatting"
            ]
}
#endif

#load ".build/boot.fsx"

open System.IO
open Fake 
open Fake.AssemblyInfoFile
open Fake.MSBuild
open FSharp.Literate
open FSharp.MetadataFormat

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
let docsDir = __SOURCE_DIRECTORY__ @@ "docs"
let referenceDir = docsDir @@ "reference"
let deployDir = __SOURCE_DIRECTORY__ @@ "deploy"
let packagesDir = __SOURCE_DIRECTORY__ @@ "packages"
let sources = __SOURCE_DIRECTORY__ @@ "src"

(* tools *)
let nugetPath = ".nuget/NuGet.exe"
let toolsPath = __SOURCE_DIRECTORY__ @@ "tools"

(* Targets *)
Target "RestorePackages" (fun _ ->

    let RestorePackageParamF = 
        fun _ ->{ ToolPath = nugetPath
                  Sources = []
                  TimeOut = System.TimeSpan.FromMinutes 5.
                  OutputPath = "./packages" 
                } :Fake.RestorePackageHelper.RestorePackageParams

    let RestorePackages2() = 
        !! "./**/packages.config"
        |> Seq.iter (RestorePackage RestorePackageParamF)

    RestorePackages2()
)

Target "Clean" (fun _ ->
    CleanDirs [buildDir; docsDir; referenceDir; deployDir]
)

Target "BuildApp" (fun _ -> 
    let appReferences =
        !+ "src/**/*.fsproj" 
            |> Scan

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

Target "GenerateDocs" (fun _ ->
    let buildReference () =
        MetadataFormat.Generate(buildDir @@ "VegaHub.dll", referenceDir, toolsPath @@ "reference")

    buildReference()
)

Target "CopyLicense" (fun _ ->
    [ "LICENSE.txt" ] |> CopyTo buildDir
)

Target "Deploy" DoNothing
Target "Default" DoNothing

(* Build Order *)
"Clean"
    ==> "RestorePackages"
    ==> "BuildApp"
    ==> "GenerateDocs" <=> "CopyLicense"
    ==> "Deploy"

"Default" <== ["Deploy"]

// start build
RunTargetOrDefault "Default"

