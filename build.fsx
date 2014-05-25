// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#I "packages/FAKE/tools/"
#r "FakeLib.dll"

#if MONO
#else
#load "packages/SourceLink.Fake/tools/SourceLink.fsx"
#endif

open System
open System.IO
open Fake 
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.ReleaseNotesHelper

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let (!!) includes = (!! includes).SetBaseDirectory __SOURCE_DIRECTORY__

// --------------------------------------------------------------------------------------
// Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package 
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project 
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "VegaHub"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "A self-hosted SignalR hub utility for pushing Vega specs to a browser window from F# Interactive."

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = """
  A self-hosted SignalR hub utility for pushing Vega specs to a browser window from F# Interactive."""
// List of author names (for NuGet package)
let authors = [ "Ryan Riley"; "Mathias Brandewinder" ]
// Tags for your project (for NuGet package)
let tags = "F# fsharp web http vega d3 visualization"

// File system information 
// (<projectFile>.*proj is built during the building process)
let projectFile = "VegaHub"
// Pattern specifying assemblies to be tested using NUnit
let testAssemblies = "bin/VegaHub*Samples*exe"

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted 
let gitHome = "git@github.com:panesofglass"
// The name of the project on GitHub
let gitName = "VegaHub"
let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/panesofglass"

// --------------------------------------------------------------------------------------
// The rest of the file includes standard build steps 
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
let release = parseReleaseNotes (IO.File.ReadAllLines "RELEASE_NOTES.md")
let isAppVeyorBuild = environVar "APPVEYOR" <> null
let nugetVersion = 
    if isAppVeyorBuild then sprintf "%s.%s" release.NugetVersion buildVersion
    else release.NugetVersion

// Generate assembly info files with the right version & up-to-date information
Target "AssemblyInfo" (fun _ ->
  let fileName = "src/" + project + "/AssemblyInfo.fs"
  CreateFSharpAssemblyInfo fileName
      [ Attribute.Title project
        Attribute.Product project
        Attribute.Description summary
        Attribute.Version release.AssemblyVersion
        Attribute.FileVersion release.AssemblyVersion ] )

Target "BuildVersion" (fun _ ->
    Shell.Exec("appveyor", sprintf "UpdateBuild -Version \"%s\"" nugetVersion) |> ignore
)

// --------------------------------------------------------------------------------------
// Clean build results & restore NuGet packages

Target "RestorePackages" RestorePackages

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "temp"]
)

Target "CleanDocs" (fun _ ->
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
    !! ("*/**/" + projectFile + "*.*proj")
    |> MSBuildRelease "bin" "Rebuild"
    |> ignore
)

#if MONO
Target "SourceLink" <| id
#else
open SourceLink

Target "SourceLink" (fun _ ->
    use repo = new GitRepo(__SOURCE_DIRECTORY__)
    !! ("*/**/" + projectFile + "*.*proj")
    |> Seq.iter (fun f ->
        let proj = VsProj.Load f ["Configuration", "Release"; "OutputPath", Path.combine __SOURCE_DIRECTORY__ "bin"]
        logfn "source linking %s" proj.OutputFilePdb
        let files = proj.Compiles -- "**/AssemblyInfo.fs"
        repo.VerifyChecksums files
        proj.VerifyPdbChecksums files
        proj.CreateSrcSrv (sprintf "%s/%s/{0}/%%var2%%" gitRaw gitName) repo.Revision (repo.Paths files)
        Pdbstr.exec proj.OutputFilePdb proj.OutputFilePdbSrcSrv
    )
)
#endif

Target "CopyLicense" (fun _ ->
    [ "LICENSE.txt" ] |> CopyTo "bin"
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "RunTests" (fun _ ->
    !! testAssemblies
    |> NUnit (fun p ->
        { p with
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

let referenceDependencies dependencies =
    let packagesDir = __SOURCE_DIRECTORY__  @@ "packages"
    [ for dependency in dependencies -> dependency, GetPackageVersion packagesDir dependency ]

Target "NuGet" (fun _ ->
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = project
            Summary = summary
            Description = description
            Version = release.NugetVersion
            ReleaseNotes = String.Join(Environment.NewLine, release.Notes)
            Tags = tags
            OutputPath = "bin"
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            Dependencies = referenceDependencies
                            [
                                "ImpromptuInterface.FSharp"
                                "Microsoft.Owin.Hosting"
                                "Microsoft.Owin.Host.HttpListener"
                                "Microsoft.Owin.StaticFiles"
                                "Microsoft.AspNet.SignalR.Core"
                                "jQuery"
                                "Microsoft.AspNet.SignalR.JS"
                            ]
        })
        ("nuget/" + project + ".nuspec")
)

// --------------------------------------------------------------------------------------
// Generate the documentation

Target "GenerateDocs" (fun _ ->
    executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"] [] |> ignore
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target "ReleaseDocs" (fun _ ->
    let ghPages = "gh-pages"
    CleanDir ghPages
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") ghPages ghPages

    fullclean ghPages
    CopyRecursive "docs/output" ghPages true |> tracefn "%A"
    StageAll ghPages
    Commit ghPages (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push ghPages
)

Target "Release" DoNothing

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  =?> ("BuildVersion", isAppVeyorBuild)
  ==> "RestorePackages"
  ==> "AssemblyInfo"
  ==> "Build"
  =?> ("SourceLink", not isMono && not (hasBuildParam "skipSourceLink"))
  ==> "CopyLicense"
  ==> "RunTests"
  =?> ("NuGet", not isMono)
  ==> "All"

"All" 
  ==> "CleanDocs"
  ==> "GenerateDocs"
  ==> "ReleaseDocs"
  ==> "Release"

RunTargetOrDefault "All"
