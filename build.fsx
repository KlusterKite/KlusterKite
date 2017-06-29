(* -- Fake Dependencies paket.dependencies
file ./paket.dependencies
group netcorebuild
-- Fake Dependencies -- *)

//#load ".fake/build.fsx/loadDependencies.fsx"
// TODO: remove dll load after loadDependencies.fsx fixed in fake

#r ".fake/build.fsx/packages/netcorebuild/Fake.Core.Targets/lib/netstandard1.6/Fake.Core.Targets.dll"
#r ".fake/build.fsx/packages/netcorebuild/Fake.Core.Environment/lib/netstandard1.6/Fake.Core.Environment.dll"
#r ".fake/build.fsx/packages/netcorebuild/Fake.Core.Process/lib/netstandard1.6/Fake.Core.Process.dll"
#r ".fake/build.fsx/packages/netcorebuild/Fake.Core.SemVer/lib/netstandard1.6/Fake.Core.SemVer.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.Diagnostics.Process/runtimes/win/lib/netstandard1.4/System.Diagnostics.Process.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.Text.Encoding/ref/netstandard1.3/System.Text.Encoding.dll"
#r ".fake/build.fsx/packages/netcorebuild/Fake.DotNet.NuGet/lib/netstandard1.6/Fake.DotNet.NuGet.dll"
#r ".fake/build.fsx/packages/netcorebuild/Fake.DotNet.MsBuild/lib/netstandard1.6/Fake.DotNet.MsBuild.dll"
#r ".fake/build.fsx/packages/netcorebuild/Fake.IO.FileSystem/lib/netstandard1.6/Fake.IO.FileSystem.dll"
#r ".fake/build.fsx/packages/netcorebuild/Microsoft.Build/lib/netstandard1.5/Microsoft.Build.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.Xml.ReaderWriter/lib/netstandard1.3/System.Xml.ReaderWriter.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.Xml.XmlDocument/lib/netstandard1.3/System.Xml.XmlDocument.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.Xml.XDocument/lib/netstandard1.3/System.Xml.XDocument.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.IO.Compression/runtimes/win/lib/netstandard1.3/System.IO.Compression.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.IO.Compression.ZipFile/lib/netstandard1.3/System.IO.Compression.ZipFile.dll"
#r ".fake/build.fsx/packages/netcorebuild/System.Net.Http/runtimes/win/lib/netstandard1.3/System.Net.Http.dll"

#r @".fake/build.fsx/packages/netcorebuild/Newtonsoft.Json/lib/netstandard1.3/Newtonsoft.Json.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Protocol.Core.Types/lib/netstandard1.3/NuGet.Protocol.Core.Types.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Protocol.Core.v3/lib/netstandard1.3/NuGet.Protocol.Core.v3.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Configuration/lib/netstandard1.3/NuGet.Configuration.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Common/lib/netstandard1.3/NuGet.Common.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Frameworks/lib/netstandard1.3/NuGet.Frameworks.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Versioning/lib/netstandard1.0/NuGet.Versioning.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Packaging/lib/netstandard1.3/NuGet.Packaging.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Packaging.Core/lib/netstandard1.3/NuGet.Packaging.Core.dll"
#r @".fake/build.fsx/packages/netcorebuild/NuGet.Packaging.Core.Types/lib/netstandard1.3/NuGet.Packaging.Core.Types.dll"

#load "./build.base.fsx"
open ClusterKit.Build.Base

open System
open System.IO
open System.Diagnostics

open Fake.Core
open Fake.Core.TargetOperators
open Fake.Core.Environment
open Fake.Core.Process
open Fake.DotNet.MsBuild
open Fake.IO.FileSystem.Shell


// builds base (system) docker images
Target.Create "DockerBase" (fun _ ->
    buildDocker "clusterkit/baseworker" "Docker/ClusterKitBaseWorkerNode"
    buildDocker "clusterkit/baseweb" "Docker/ClusterKitBaseWebNode"
    buildDocker "clusterkit/nuget" "Docker/ClusterKitNuget"
    buildDocker "clusterkit/postgres" "Docker/ClusterKitPostgres"
    buildDocker "clusterkit/entry" "Docker/ClusterKitEntry"
    buildDocker "clusterkit/vpn" "Docker/ClusterKitVpn"
    buildDocker "clusterkit/elk" "Docker/ClusterKitELK"
    buildDocker "clusterkit/redis" "Docker/ClusterKit.Redis"
)

// builds standard docker images
Target.Create "DockerContainers" (fun _ ->
    
    let buildProject (outputPath:string) (projectPath:string) = 
        let setParams defaults = { 
                defaults with
                    Verbosity = Some(Minimal)
                    Targets = ["Restore"; "Publish"]
                    RestorePackagesFlag = true
                    Properties = 
                    [
                        "Optimize", "True"
                        "DebugSymbols", "True"
                        "Configuration", "Release"
                        "TargetFramework", "netcoreapp1.1"
                        "OutputPath", outputPath
                    ]
                }
        dotNetBuild setParams projectPath

    CleanDirs [|"./build/launcher"; "./build/launcherpublish"; "./build/seed"; "./build/seedpublish"; "./build/seeder"; "./build/seederpublish";|]
    buildProject (Path.GetFullPath "./build/launcher") "./build/src/ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj"
    
    // MSBuildRelease "./build/seed" "Build" [|"./ClusterKit.Log/ClusterKit.Log.Console/ClusterKit.Log.Console.csproj"|] |> ignore
    // MSBuildRelease "./build/seed" "Build" [|"./ClusterKit.Log/ClusterKit.Log.ElasticSearch/ClusterKit.Log.ElasticSearch.csproj"|] |> ignore
    // buildProject (Path.GetFullPath "./build/seed") "./ClusterKit.Monitoring/ClusterKit.Monitoring.Client/ClusterKit.Monitoring.Client.csproj"
    buildProject (Path.GetFullPath "./build/seed") "./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj"

    buildProject (Path.GetFullPath "./build/seeder") "./ClusterKit.NodeManager/ClusterKit.NodeManager.Seeder.Launcher/ClusterKit.NodeManager.Seeder.Launcher.csproj"
    

    let copyLauncherData (path : string) =
        let fullPath = Path.GetFullPath(path)
        let buildDir = Path.Combine ([|fullPath; "build"|])
        let packageCacheDir = Path.Combine ([|fullPath; "packageCache"|])

        CleanDirs [|buildDir; packageCacheDir|]
        CopyDir buildDir "./build/launcherpublish" (fun _ -> true)
        CopyTo buildDir [|"./Docker/utils/launcher/start.sh"|]
        CopyTo buildDir [|"./nuget.exe"|]
        

    CleanDirs [|"./Docker/ClusterKitSeed/build"|]
    CopyDir "./Docker/ClusterKitSeed/build" "./build/seedpublish" (fun _ -> true)
    buildDocker "clusterkit/seed" "Docker/ClusterKitSeed"

    CleanDirs [|"./Docker/ClusterKitSeeder/build"|]
    CopyDir "./Docker/ClusterKitSeeder/build" "./build/seederpublish" (fun _ -> true)
    buildDocker "clusterkit/seeder" "Docker/ClusterKitSeeder"

    copyLauncherData "./Docker/ClusterKitWorker" |> ignore
    copyLauncherData "./Docker/ClusterKitPublisher" |> ignore
    buildDocker "clusterkit/worker" "Docker/ClusterKitWorker"
    buildDocker "clusterkit/manager" "Docker/ClusterKitManager"

    buildDocker "clusterkit/publisher" "Docker/ClusterKitPublisher"
    
    // building node.js web sites
    Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env-local" "./Docker/ClusterKitMonitoring/clusterkit-web/.env"
    Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env" "./Docker/ClusterKitMonitoring/clusterkit-web/.env-build"
    
    /// Default paths to Npm
    let npmFileName =
        match isWindows with
        | true -> 
            System.Environment.GetEnvironmentVariable("PATH")
            |> fun path -> path.Split ';'
            |> Seq.tryFind (fun p -> p.Contains "nodejs")
            |> fun res ->
                match res with
                | Some npm when File.Exists (sprintf @"%snpm.cmd" npm) -> (sprintf @"%snpm.cmd" npm)
                | _ -> "./fake/build.fsx/packages/netcorebuild/Npm/content/.bin/npm.cmd"
        | _ -> 
            let info = new ProcessStartInfo("which","npm")
            info.StandardOutputEncoding <- System.Text.Encoding.UTF8
            info.RedirectStandardOutput <- true
            info.UseShellExecute        <- false
            info.CreateNoWindow         <- true
            use proc = Process.Start info
            proc.WaitForExit()
            match proc.ExitCode with
                | 0 when not proc.StandardOutput.EndOfStream ->
                  proc.StandardOutput.ReadLine()
                | _ -> "/usr/bin/npm"

    printfn "Running: %s install" npmFileName
    if not(execProcess (fun info -> 
                info.UseShellExecute <- false
                info.WorkingDirectory <- "./Docker/ClusterKitMonitoring/clusterkit-web" 
                info.FileName <- npmFileName
                info.Arguments <- "install") (TimeSpan.FromDays 2.0)) then failwith "Could not install npm modules"
    printfn "Running: %s run build" npmFileName
    if not(execProcess (fun info -> 
                info.UseShellExecute <- false
                info.WorkingDirectory <- "./Docker/ClusterKitMonitoring/clusterkit-web" 
                info.FileName <- npmFileName
                info.Arguments <- "run build") (TimeSpan.FromDays 2.0)) then failwith "Could build clusterkit-web"

    buildDocker "clusterkit/monitoring-ui" "Docker/ClusterKitMonitoring"
    
    Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env-build" "./Docker/ClusterKitMonitoring/clusterkit-web/.env"
    Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env" "./Docker/ClusterKitMonitoring/clusterkit-web/.env-local"
)

"PrepareSources" ==> "DockerContainers"
"DockerBase" ?=> "CleanDockerImages"
"DockerContainers" ?=> "CleanDockerImages"
"DockerBase" ?=> "DockerContainers"

// prepares docker images
Target.Create  "FinalBuildDocker" (fun _ -> ())
"DockerBase" ==> "FinalBuildDocker"
"DockerContainers" ==> "FinalBuildDocker"
"CleanDockerImages" ==> "FinalBuildDocker"

Target.RunOrDefault "Nuget"
