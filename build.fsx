#r "./packages/FAKE/tools/FakeLib.dll" // include Fake lib
#r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils
open Fake
open System
open System.IO
open System.Xml
open System.Linq

open  ClusterKit.Build

let buildDir = Path.GetFullPath("./build")
let packageDir = Path.GetFullPath("./packageOut")
let ver = environVar "version"

let currentTarget = getBuildParam "target"

BuildUtils.Configure(ver, buildDir, packageDir, "./packages")

let projects = [|
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Build/ClusterKit.Build.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Tests/ClusterKit.Core.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.EF/ClusterKit.Core.EF.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.EF.Npgsql/ClusterKit.Core.EF.Npgsql.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"|]))

    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Descriptor/ClusterKit.Web.Descriptor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.SignalR/ClusterKit.Web.SignalR.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Swagger.Messages/ClusterKit.Web.Swagger.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage, ([||]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Swagger/ClusterKit.Web.Swagger.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Swagger.Monitor/ClusterKit.Web.Swagger.Monitor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Tests/ClusterKit.Web.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web.NginxConfigurator"; "ClusterKit.Web.SignalR"|]))

    new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring/ClusterKit.Monitoring.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.SignalR"|]))
    new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring.Tests/ClusterKit.Monitoring.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core";  "ClusterKit.Core.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web"|]))

    new ProjectDescription("./ClusterKit.Extensions/ClusterKit.Guarantee/ClusterKit.Guarantee.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Extensions/ClusterKit.BusinessObjects/ClusterKit.BusinessObjects.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Extensions/ClusterKit.Extensions.Tests/ClusterKit.Extensions.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core";  "ClusterKit.Core.TestKit"; "ClusterKit.Guarantee"; "ClusterKit.BusinessObjects"|]))

    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Client/ClusterKit.NodeManager.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.ConfigurationSource/ClusterKit.NodeManager.ConfigurationSource.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager/ClusterKit.NodeManager.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.NodeManager.Client"; "ClusterKit.NodeManager.ConfigurationSource"; "ClusterKit.Core.EF"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Tests/ClusterKit.NodeManager.Tests.csproj", ProjectDescription.EnProjectType.SimpleBuild, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.NodeManager.Client"; "ClusterKit.NodeManager.ConfigurationSource"; "ClusterKit.Core.EF"; "ClusterKit.Core.EF.Npgsql"|]))

|]

Target "PreClean" (fun _ ->
    trace "PreClean..."
    if Directory.Exists(packageDir) then Directory.Delete(packageDir, true)
    if Directory.Exists(buildDir) then Directory.Delete(buildDir, true)
    Directory.CreateDirectory(buildDir) |> ignore
    Directory.CreateDirectory(Path.Combine(buildDir, "tmp")) |> ignore
    Directory.CreateDirectory(Path.Combine(buildDir, "clean")) |> ignore
)

Target "Build"  (fun _ ->
    BuildUtils.Build(projects, true);
)

Target "PublishNuGet" (fun _ ->
    BuildUtils.CreateNuget(projects);
)

Target "ReloadNuGet" (fun _ ->
    BuildUtils.ReloadNuget(projects);
)

Target "Test" (fun _ ->
    BuildUtils.RunXUnitTest(projects);
)

"PreClean" ==> "Build"
"Build" ==> "PublishNuGet"
"PublishNuGet" ==> "ReloadNuGet"
"Build" ==> "Test"

RunTargetOrDefault "PreClean"