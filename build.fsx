#r "./packages/FAKE/tools/FakeLib.dll" // include Fake lib
#r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils
open Fake
open System
open System.IO
open System.Xml
open System.Linq
open System.Text.RegularExpressions

open  ClusterKit.Build

let buildDir = Path.GetFullPath("./build")
let packageDir = Path.GetFullPath("./packageOut")
let ver = environVar "version"

let currentTarget = getBuildParam "target"

BuildUtils.Configure(ver, buildDir, packageDir, "./packages")

let projects = [|
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Build/ClusterKit.Build.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Rest/ClusterKit.Core.Rest.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Tests/ClusterKit.Core.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.EF/ClusterKit.Core.EF.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.EF.Npgsql/ClusterKit.Core.EF.Npgsql.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"|]))

    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Descriptor/ClusterKit.Web.Descriptor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.CRUDS/ClusterKit.Web.CRUDS.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
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

    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher.Messages/ClusterKit.NodeManager.Launcher.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Client/ClusterKit.NodeManager.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.ConfigurationSource/ClusterKit.NodeManager.ConfigurationSource.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager/ClusterKit.NodeManager.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.NodeManager.Client"; "ClusterKit.NodeManager.ConfigurationSource"; "ClusterKit.Core.EF"; "ClusterKit.Core.Rest"; "ClusterKit.Web.CRUDS"; "ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Tests/ClusterKit.NodeManager.Tests.csproj", ProjectDescription.EnProjectType.SimpleBuild, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.Web.CRUDS"; "ClusterKit.NodeManager.Client"; "ClusterKit.NodeManager.ConfigurationSource"; "ClusterKit.Core.EF"; "ClusterKit.Core.EF.Npgsql"; "ClusterKit.Core.Rest"; "ClusterKit.NodeManager.Launcher.Messages"|]))

|]

let buildDocker (containerName:string) (path:string) =
    if (ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- (sprintf "build -t %s:latest %s" containerName path)
    )  (TimeSpan.FromMinutes 30.0) <> 0) then
        failwithf "Error while building %s" path

let pushPackage package =
    ExecProcess (fun info ->
        info.FileName <- "nuget.exe";
        info.Arguments <- sprintf "push %s -Source %s -ApiKey %s" package "http://192.168.99.100:81/" "ClusterKit")
        (TimeSpan.FromMinutes 30.0)
        |> ignore

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

Target "DockerBase" (fun _ ->
    buildDocker "clusterkit/baseworker" "Docker/ClusterKitBaseWorkerNode"
    buildDocker "clusterkit/baseweb" "Docker/ClusterKitBaseWebNode"
)

Target "DockerContainers" (fun _ ->
    RestorePackages |> ignore
    MSBuildRelease "./build/launcher" "Build" [|"./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj"|] |> ignore

    let copyLauncherData (path : string) =
        let fullPath = Path.GetFullPath(path)
        let buildDir = Path.Combine ([|fullPath; "build"|])
        let packageCacheDir = Path.Combine ([|fullPath; "packageCache"|])

        Fake.FileHelper.CleanDirs [|buildDir; packageCacheDir|]
        Fake.FileHelper.CopyDir buildDir "./build/launcher" (fun file -> true)

        let copyThirdPartyPackage (f: FileInfo) =
            if (hasExt ".nupkg" f.FullName) then
                if not (File.Exists (Path.Combine [|(Path.GetFullPath("./packageOut/")); f.Name|])) then
                    Fake.FileHelper.Copy packageCacheDir [|f.FullName|]

        Fake.FileHelper.recursively
            (fun d -> ())
            copyThirdPartyPackage
            (new DirectoryInfo(Path.GetFullPath("./packages")))

    let copyWebContent source dest =
        let fullPathSource = Path.GetFullPath(source)
        let fullPathDest = Path.GetFullPath(dest)
        Fake.FileHelper.CleanDirs [|fullPathDest|]
        let matcher name = Regex.IsMatch(name, "(.*)((\.jpg)|(\.gif)|(\.png)|(\.jpeg)|(\.html)|(\.html)|(\.js)|(\.css))$", RegexOptions.IgnoreCase)
        Fake.FileHelper.CopyDir fullPathDest fullPathSource matcher

    copyLauncherData "./Docker/ClusterKitWorker" |> ignore
    copyLauncherData "./Docker/ClusterKitSeed" |> ignore
    buildDocker "clusterkit/worker" "Docker/ClusterKitWorker"
    buildDocker "clusterkit/manager" "Docker/ClusterKitManager"

    copyWebContent "./ClusterKit.Monitoring/ClusterKit.Monitoring.Web" "./Docker/ClusterKitSeed/web/monitoring"
    buildDocker "clusterkit/seed" "Docker/ClusterKitSeed"
)

Target "Proof" (fun _ ->
    let nugetVersion = Fake.NuGetVersion.getLastNuGetVersion "http://192.168.99.100:81" "ClusterKit.Core"
    let version = if nugetVersion.IsSome then (Fake.NuGetVersion.IncPatch nugetVersion.Value) else (SemVerHelper.parse "0.0.0")

    trace (version.ToString())
)

Target "CleanDockerImages" (fun _ ->

    let outputProcess line =
        let parts = Regex.Split(line, "[\t ]+")
        if ("<none>".Equals(parts.[0]) && parts.Length >= 3) then
            let args = sprintf "rmi %s" parts.[2]
            ExecProcess (fun info -> info.FileName <- "docker"; info.Arguments <- args) (TimeSpan.FromMinutes 30.0)
                |> ignore

    let lines = new ResizeArray<String>();
    ExecProcessWithLambdas
        (fun info -> info.FileName <- "docker"; info.Arguments <- "images")
        (TimeSpan.FromMinutes 30.0)
        true
        (fun e -> failwith e)
        (fun l -> lines.Add(l))
        |> ignore

    lines |> Seq.iter outputProcess
)

Target "PushPackages" (fun _ ->
    let pushThirdPartyPackage (f: FileInfo) =
            if (hasExt ".nupkg" f.FullName) then
                if not (File.Exists (Path.Combine [|(Path.GetFullPath("./packageOut/")); f.Name|])) then
                    pushPackage f.FullName

    Fake.FileHelper.recursively
            (fun d -> ())
            pushThirdPartyPackage
            (new DirectoryInfo(Path.GetFullPath("./packages")))

    Directory.GetFiles(Path.GetFullPath("./packageOut"))
        |> Seq.filter (hasExt ".nupkg")
        |> Seq.iter pushPackage

)

"PreClean" ==> "Build"
"Build" ==> "PublishNuGet"
"PublishNuGet" ==> "ReloadNuGet"
"Build" ==> "Test"

RunTargetOrDefault "PreClean"