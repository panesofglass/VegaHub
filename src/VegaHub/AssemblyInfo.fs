namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("VegaHub")>]
[<assembly: AssemblyProductAttribute("VegaHub")>]
[<assembly: AssemblyDescriptionAttribute("A self-hosted SignalR hub utility for pushing Vega specs to a browser window from F# Interactive.")>]
[<assembly: AssemblyVersionAttribute("1.0.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0.0"
