#r "./packages/FAKE/tools/FakeLib.dll" // include Fake lib
#r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils
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
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Build/ClusterKit.Build.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Tests/ClusterKit.Core.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.SignalR/ClusterKit.Web.SignalR.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Tests/ClusterKit.Web.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web.NginxConfigurator"; "ClusterKit.Web.SignalR"|]))
    new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring/ClusterKit.Monitoring.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"|]))
    new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring.Tests/ClusterKit.Monitoring.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core";  "ClusterKit.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web"|]))
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

Target "Test" (fun _ ->
    BuildUtils.RunXUnitTest(projects);
)

"PreClean" ==> "Build"
"Build" ==> "PublishNuGet"
"Build" ==> "Test"

RunTargetOrDefault "PreClean"