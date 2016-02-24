#r "./packages/FAKE/tools/FakeLib.dll" // include Fake lib
#r @"BuildScripts/ClusterKit.Build.dll"
open Fake
open System
open System.IO
open System.Xml
open System.Linq

open  ClusterKit.Build

let buildDir = "./build"
let packageDir = "./packageOut"
let ver = environVar "version"

let currentTarget = getBuildParam "target"

BuildUtils.Configure(ver, buildDir, packageDir)

let projects = [|
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj")
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.SignalR/ClusterKit.Web.SignalR.csproj", ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring/ClusterKit.Monitoring.csproj", ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"|]))
|]

Target "PreClean" (fun _ ->
    trace "PreClean..."
    if Directory.Exists(packageDir) then Directory.Delete(packageDir, true)
)

Target "Build"  (fun _ ->
    BuildUtils.Build(projects);
)

Target "PublishNuGet" (fun _ ->
    BuildUtils.CreateNuget(projects);
)

"PreClean" ==> "Build"
"Build" ==> "PublishNuGet"

RunTargetOrDefault "PreClean"