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

BuildUtils.Configure((if ver <> null then ver else "0.0.0.0-local"), buildDir, packageDir, "./packages")

let projects = [|
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Build/ClusterKit.Build.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Rest/ClusterKit.Core.Rest.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Data/ClusterKit.Core.Data.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core.Rest"|]))

    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Data.TestKit/ClusterKit.Core.Data.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core.Rest"; "ClusterKit.Core.Data"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.EF/ClusterKit.Core.EF.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Core.Data"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.EF.TestKit/ClusterKit.Core.EF.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Core.Data"; "ClusterKit.Core.EF"|]))
    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.EF.Npgsql/ClusterKit.Core.EF.Npgsql.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"; "ClusterKit.Core.Data"|]))

    new ProjectDescription("./ClusterKit.Core/ClusterKit.Core.Tests/ClusterKit.Core.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.Data"; "ClusterKit.Core.EF"; "ClusterKit.Core.EF.Npgsql"; "ClusterKit.Core.Rest"; "ClusterKit.Core.TestKit"|]))

    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Descriptor/ClusterKit.Web.Descriptor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.CRUDS/ClusterKit.Web.CRUDS.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.SignalR/ClusterKit.Web.SignalR.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Swagger.Messages/ClusterKit.Web.Swagger.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage, ([||]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Swagger/ClusterKit.Web.Swagger.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Swagger.Monitor/ClusterKit.Web.Swagger.Monitor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"|]))
    new ProjectDescription("./ClusterKit.Web/ClusterKit.Web.Tests/ClusterKit.Web.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web.NginxConfigurator"; "ClusterKit.Web.SignalR"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web.Swagger.Messages"; "ClusterKit.Web.Swagger.Monitor"; "ClusterKit.Web.Swagger"; "ClusterKit.Web"|]))

    new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring/ClusterKit.Monitoring.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.SignalR"|]))
    //new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring.Tests/ClusterKit.Monitoring.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core";  "ClusterKit.Core.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.SignalR"|]))

    new ProjectDescription("./ClusterKit.Extensions/ClusterKit.Guarantee/ClusterKit.Guarantee.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Extensions/ClusterKit.BusinessObjects/ClusterKit.BusinessObjects.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("./ClusterKit.Extensions/ClusterKit.Extensions.Tests/ClusterKit.Extensions.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core";  "ClusterKit.Core.TestKit"; "ClusterKit.Guarantee"; "ClusterKit.BusinessObjects"|]))

    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher.Messages/ClusterKit.NodeManager.Launcher.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.Client/ClusterKit.NodeManager.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("./ClusterKit.NodeManager/ClusterKit.NodeManager.ConfigurationSource/ClusterKit.NodeManager.ConfigurationSource.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"; "ClusterKit.Core.Data"|]))

    new ProjectDescription(
        "./ClusterKit.NodeManager/ClusterKit.NodeManager/ClusterKit.NodeManager.csproj",
        ProjectDescription.EnProjectType.NugetPackage,
        ([|
            "ClusterKit.Core";
            "ClusterKit.Core.EF";
            "ClusterKit.Core.Data";
            "ClusterKit.Core.Rest";
            "ClusterKit.Web.Client";
            "ClusterKit.Web.Descriptor";
            "ClusterKit.Web";
            "ClusterKit.Web.CRUDS";
            "ClusterKit.NodeManager.Client";
            "ClusterKit.NodeManager.ConfigurationSource";
            "ClusterKit.NodeManager.Launcher.Messages"
            |]))
    new ProjectDescription(
        "./ClusterKit.NodeManager/ClusterKit.NodeManager.Tests/ClusterKit.NodeManager.Tests.csproj",
        ProjectDescription.EnProjectType.XUnitTests,
        ([|
            "ClusterKit.Core";
            "ClusterKit.Core.TestKit";
            "ClusterKit.Core.Rest";
            "ClusterKit.Core.Data";
            "ClusterKit.Core.Data.TestKit";
            "ClusterKit.Core.EF";
            "ClusterKit.Core.EF.TestKit";
            "ClusterKit.Core.EF.Npgsql";
            "ClusterKit.Web.Client";
            "ClusterKit.Web.Descriptor";
            "ClusterKit.Web";
            "ClusterKit.Web.CRUDS";
            "ClusterKit.NodeManager";
            "ClusterKit.NodeManager.Client";
            "ClusterKit.NodeManager.ConfigurationSource";
            "ClusterKit.NodeManager.Launcher.Messages"
          |]))

|]

let buildDocker (containerName:string) (path:string) =
    if (ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- (sprintf "build -t %s:latest %s" containerName path)
    )  (TimeSpan.FromMinutes 30.0) <> 0) then
        failwithf "Error while building %s" path

let pushPackage package =
    trace package
    let localPath = Fake.FileSystemHelper.currentDirectory
    trace localPath
    let packageLocal = package.Replace(localPath, ".")
    trace packageLocal
    ExecProcess (fun info ->
        info.FileName <- "nuget.exe";
        info.Arguments <- sprintf "push %s -Source %s -ApiKey %s" packageLocal "http://192.168.99.100:81/" "ClusterKit")
        (TimeSpan.FromMinutes 30.0)
        |> ignore

// This target removes all temp and build result files
Target "Clean" (fun _ ->
    trace "PreClean..."
    Fake.FileHelper.CleanDirs [|
        packageDir
        buildDir
        Path.Combine(buildDir, "tmp")
        Path.Combine(buildDir, "clean")
        |]
)

// perfoms global project compilation
Target "Build"  (fun _ ->
    BuildUtils.Build(projects);
)

// creates nuget package for every project
Target "CreateNuGet" (fun _ ->
    Fake.FileHelper.CleanDirs [|packageDir|]
    BuildUtils.CreateNuget(projects);
)

// removes installed internal package from packages directory and restores them from latest build
Target "RefreshLocalDependencies" (fun _ ->
    BuildUtils.ReloadNuget(projects);
)

// runs all xunit tests
Target "Test" (fun _ ->
    BuildUtils.RunXUnitTest(projects);
)

// builds base (system) docker images
Target "DockerBase" (fun _ ->
    buildDocker "clusterkit/baseworker" "Docker/ClusterKitBaseWorkerNode"
    buildDocker "clusterkit/baseweb" "Docker/ClusterKitBaseWebNode"
    buildDocker "clusterkit/nuget" "Docker/ClusterKitNuget"
    buildDocker "clusterkit/postgres" "Docker/ClusterKitPostgres"
    buildDocker "clusterkit/entry" "Docker/ClusterKitEntry"
)

// builds standard docker images
Target "DockerContainers" (fun _ ->
    RestorePackages |> ignore
    MSBuildRelease "./build/launcher" "Build" [|"./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj"|] |> ignore
    MSBuildRelease "./build/seed" "Build" [|"./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj"|] |> ignore

    let copyLauncherData (path : string) =
        let fullPath = Path.GetFullPath(path)
        let buildDir = Path.Combine ([|fullPath; "build"|])
        let packageCacheDir = Path.Combine ([|fullPath; "packageCache"|])

        Fake.FileHelper.CleanDirs [|buildDir; packageCacheDir|]
        Fake.FileHelper.CopyDir buildDir "./build/launcher" (fun file -> true)
        Fake.FileHelper.CopyTo buildDir [|"./Docker/utils/launcher/start.sh"|]

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

    Fake.FileHelper.CleanDirs [|"./Docker/ClusterKitSeed/build"|]
    Fake.FileHelper.CopyDir "./Docker/ClusterKitSeed/build" "./build/seed" (fun file -> true)
    buildDocker "clusterkit/seed" "Docker/ClusterKitSeed"

    copyLauncherData "./Docker/ClusterKitWorker" |> ignore
    copyLauncherData "./Docker/ClusterKitPublisher" |> ignore
    buildDocker "clusterkit/worker" "Docker/ClusterKitWorker"
    buildDocker "clusterkit/manager" "Docker/ClusterKitManager"

    copyWebContent "./ClusterKit.Monitoring/ClusterKit.Monitoring.Web" "./Docker/ClusterKitPublisher/web/monitoring"
    buildDocker "clusterkit/publisher" "Docker/ClusterKitPublisher"

    buildDocker "clusterkit/monitoring-ui" "Docker/ClusterKitMonitoring"
)

// removes unnamed dockaer images
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

// sends prepared packages to docker nuget server
Target "PushThirdPartyPackages" (fun _ ->
    let pushThirdPartyPackage (f: FileInfo) =
            if (hasExt ".nupkg" f.FullName) then
                if not (File.Exists (Path.Combine [|(Path.GetFullPath("./packageOut/")); f.Name|])) then
                    pushPackage f.FullName

    Fake.FileHelper.recursively
            (fun d -> ())
            pushThirdPartyPackage
            (new DirectoryInfo(Path.GetFullPath("./packages")))

)

// sends prepared packages to docker nuget server
Target "PushLocalPackages" (fun _ ->
    Directory.GetFiles(Path.GetFullPath("./packageOut"))
        |> Seq.filter (hasExt ".nupkg")
        |> Seq.iter pushPackage
)

// switches nuget and build version from init one, to latest posible on docker nuget server
Target "SetVersion" (fun _ ->
    let nugetVersion = Fake.NuGetVersion.getLastNuGetVersion "http://192.168.99.100:81" "ClusterKit.Core"
    if nugetVersion.IsSome then tracef "Current version is %s \n" (nugetVersion.ToString()) else trace "Repository is empty"
    let version = Regex.Replace((if nugetVersion.IsSome then ((Fake.NuGetVersion.IncPatch nugetVersion.Value).ToString()) else "0.0.0-local"), "((\\d+\\.?)+)(.*)", "$1-local")
    tracef "New version is %s \n" version
    BuildUtils.Configure(version, buildDir, packageDir, "./packages")
)

// removes all installed packages and restores them (so this will remove obsolete packages)
Target "CleanPackageCache" (fun _ ->
    Directory.GetDirectories(Path.GetFullPath("./packages"))
        |> Seq.filter (fun d -> not("FAKE".Equals(d.Split(Path.DirectorySeparatorChar) |> Seq.last)))
        |> Seq.iter (fun d -> Fake.FileUtils.rm_rf d)
    RestorePackages()
)

// this was main pure build types. They don't have prerequsits by default, so you can save time on some type of operations
"Clean" ?=> "Build"
"SetVersion" ?=> "Build"

"SetVersion" ?=> "CreateNuGet"
"Build" ?=> "CreateNuGet"

"CreateNuGet" ?=> "RefreshLocalDependencies"

"Build" ?=> "Test"

"Build" ?=> "RefreshLocalDependencies"
"CreateNuGet" ?=> "RefreshLocalDependencies"

"DockerBase" ?=> "CleanDockerImages"
"DockerContainers" ?=> "CleanDockerImages"
"DockerBase" ?=> "DockerContainers"

"CleanPackageCache" <=? "CreateNuGet"

"PushLocalPackages" <=? "CreateNuGet"
"PushThirdPartyPackages" <=? "CleanPackageCache"
"PushThirdPartyPackages" <=? "CreateNuGet"
"PushThirdPartyPackages" <=? "Clean"

// from now on this will be end-point targets with respect to build order

//builds current project
Target "FinalBuild" (fun _ -> ())

"Clean" ==> "FinalBuild"
"Build" ==> "FinalBuild"

//runs all tests
Target "FinalTest" (fun _ -> ())

"FinalBuild" ==> "FinalTest"
"Test" ==> "FinalTest"

// creates local nuget packages
Target "FinalCreateNuGet" (fun _ -> ())
"FinalBuild" ==> "FinalCreateNuGet"
"CreateNuGet" ==> "FinalCreateNuGet"

// prepares docker images
Target "FinalBuildDocker" (fun _ -> ())
"CleanPackageCache" ==> "FinalBuildDocker"
"DockerBase" ==> "FinalBuildDocker"
"DockerContainers" ==> "FinalBuildDocker"
"CleanDockerImages" ==> "FinalBuildDocker"

// builds local packages and sends them to local cluster nuget server
Target "FinalPushLocalPackages" (fun _ -> ())
"SetVersion" ==> "FinalPushLocalPackages"
"FinalCreateNuGet" ==> "FinalPushLocalPackages"
"PushLocalPackages" ==> "FinalPushLocalPackages"

// builds local packages and sends them to local cluster nuget server
Target "FinalPushAllPackages" (fun _ -> ())
"FinalPushLocalPackages" ==> "FinalPushAllPackages"
"PushThirdPartyPackages" ==> "FinalPushAllPackages"

// rebuilds current project and reinstall local dependent packages
Target "FinalRefreshLocalDependencies" (fun _ -> ())
"RefreshLocalDependencies" ==> "FinalRefreshLocalDependencies"
"FinalCreateNuGet" ==> "FinalRefreshLocalDependencies"
"CleanPackageCache" ==> "FinalRefreshLocalDependencies"

RunTargetOrDefault "FinalRefreshLocalDependencies"