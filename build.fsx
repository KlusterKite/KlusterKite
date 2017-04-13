#r @"BuildScripts/FakeLib.dll" // include Fake lib
#r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils // include budle of build utils
open Fake
open System
open System.IO
open System.Text.RegularExpressions

#load "./build.config.fsx"
#load "./BuildScripts/ClusterKit.Build.fsx"

let buildDocker (containerName:string) (path:string) =
    if (ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- (sprintf "build -t %s:latest %s" containerName path)
    )  (TimeSpan.FromMinutes 30.0) <> 0) then
        failwithf "Error while building %s" path

// builds base (system) docker images
Target "DockerBase" (fun _ ->
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
Target "DockerContainers" (fun _ ->
    
    RestorePackages |> ignore
    
    MSBuildRelease "./build/launcher" "Build" [|"./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj"|] |> ignore

    // MSBuildRelease "./build/seed" "Build" [|"./ClusterKit.Log/ClusterKit.Log.Console/ClusterKit.Log.Console.csproj"|] |> ignore
    // MSBuildRelease "./build/seed" "Build" [|"./ClusterKit.Log/ClusterKit.Log.ElasticSearch/ClusterKit.Log.ElasticSearch.csproj"|] |> ignore
    MSBuildRelease "./build/seed" "Build" [|"./ClusterKit.Monitoring/ClusterKit.Monitoring.Client/ClusterKit.Monitoring.Client.csproj"|] |> ignore
    MSBuildRelease "./build/seed" "Build" [|"./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj"|] |> ignore

    MSBuildRelease "./build/seeder" "Build" [|"./ClusterKit.NodeManager/ClusterKit.NodeManager.Seeder.Launcher/ClusterKit.NodeManager.Seeder.Launcher.csproj"|] |> ignore
    

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

    Fake.FileHelper.CleanDirs [|"./Docker/ClusterKitSeed/build"|]
    Fake.FileHelper.CopyDir "./Docker/ClusterKitSeed/build" "./build/seed" (fun file -> true)
    buildDocker "clusterkit/seed" "Docker/ClusterKitSeed"

    Fake.FileHelper.CleanDirs [|"./Docker/ClusterKitSeeder/build"|]
    Fake.FileHelper.CopyDir "./Docker/ClusterKitSeeder/build" "./build/seeder" (fun file -> true)
    buildDocker "clusterkit/seeder" "Docker/ClusterKitSeeder"

    copyLauncherData "./Docker/ClusterKitWorker" |> ignore
    copyLauncherData "./Docker/ClusterKitPublisher" |> ignore
    buildDocker "clusterkit/worker" "Docker/ClusterKitWorker"
    buildDocker "clusterkit/manager" "Docker/ClusterKitManager"

    buildDocker "clusterkit/publisher" "Docker/ClusterKitPublisher"
    
    if not (directoryExists "./packages/Npm.js") then
        "Npm.js" |> RestorePackageId(fun p-> 
            { p with
                OutputPath = "./packages"
                ExcludeVersion = true})

    Fake.FileHelper.Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env-local" "./Docker/ClusterKitMonitoring/clusterkit-web/.env"
    Fake.FileHelper.Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env" "./Docker/ClusterKitMonitoring/clusterkit-web/.env-build"

    NpmHelper.Npm(fun p -> 
              { p with
                  Command = NpmHelper.Install NpmHelper.Standard
                  WorkingDirectory = "./Docker/ClusterKitMonitoring/clusterkit-web" 
              })

    NpmHelper.Npm(fun p ->
            { p with
                Command = (NpmHelper.Run "build")
                WorkingDirectory = "./Docker/ClusterKitMonitoring/clusterkit-web"
            })

    buildDocker "clusterkit/monitoring-ui" "Docker/ClusterKitMonitoring"

    Fake.FileHelper.Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env-build" "./Docker/ClusterKitMonitoring/clusterkit-web/.env"
    Fake.FileHelper.Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env" "./Docker/ClusterKitMonitoring/clusterkit-web/.env-local"
)

"DockerBase" ?=> "CleanDockerImages"
"DockerContainers" ?=> "CleanDockerImages"
"DockerBase" ?=> "DockerContainers"

// prepares docker images
Target "FinalBuildDocker" (fun _ -> ())
"CleanPackageCache" ==> "FinalBuildDocker"
"DockerBase" ==> "FinalBuildDocker"
"DockerContainers" ==> "FinalBuildDocker"
"CleanDockerImages" ==> "FinalBuildDocker"

RunTargetOrDefault "FinalRefreshLocalDependencies"