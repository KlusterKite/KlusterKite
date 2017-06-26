(* -- Fake Dependencies paket.dependencies
file ./paket.dependecies
group netcorebuild
-- Fake Dependencies -- *)

#load ".fake/build.fsx/loadDependencies.fsx"
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




open System
open System.IO
open System.Xml
open System.Text.RegularExpressions
open System.Collections.Generic;

open Microsoft.Build.Evaluation

open Fake.Core
open Fake.Core.TargetOperators
open Fake.Core.Environment
open Fake.Core.Process
open Fake.DotNet.NuGet.Version
open Fake.DotNet.MsBuild
open Fake.IO.FileSystem.Shell

open NuGet.Configuration;
open NuGet.Common;
open NuGet.Protocol;
open NuGet.Versioning;
open NuGet.Packaging;



let testPackageName = "ClusterKit.Core"
let buildDir = Path.GetFullPath("./build")
let mutable packageDir = Path.GetFullPath("./packageOut")
let packagePushDir = Path.GetFullPath("./packagePush")
let packageThirdPartyDir = Path.GetFullPath("./packageThirdPartyDir")

let envVersion = environVarOrDefault "version" null
let mutable version = if envVersion <> null then envVersion else "0.0.0-local"

// let currentTarget = getBuildParam "target"


let buildDocker (containerName:string) (path:string) =
    if not((execProcess (fun info ->
                info.FileName <- "docker"
                info.Arguments <- (sprintf "build -t %s:latest %s" containerName path)
                )  (TimeSpan.FromDays 2.0))) then
        failwithf "Error while building %s" path

let pushPackage (package:string) =
    let localPath = Path.GetFullPath(".")
    let packageLocal = package.Replace(localPath, ".")
    execProcess (fun info ->
        info.FileName <- "nuget.exe";
        info.Arguments <- sprintf "push %s -Source %s -ApiKey %s" packageLocal "http://docker:81/" "ClusterKit")
        (TimeSpan.FromMinutes 30.0)
        |> ignore

let filesInDirMatchingRecursive (pattern:string) (dir:DirectoryInfo) = 
    dir.GetFiles(pattern, SearchOption.AllDirectories)
let filesInDirMatching (pattern:string) (dir:DirectoryInfo) = 
    dir.GetFiles(pattern, SearchOption.TopDirectoryOnly)

Target.Create "Clean" (fun _ ->
    printfn "PreClean..."
    CleanDir buildDir
)

// switches nuget and build version from init one, to latest posible on docker nuget server
Target.Create "SetVersion" (fun _ ->
    let nugetVersion = getLastNuGetVersion "http://docker:81" testPackageName
    if nugetVersion.IsSome then printfn "Current version is %s " (nugetVersion.ToString()) else printfn "Repository is empty"
    version <- Regex.Replace((if nugetVersion.IsSome then ((IncPatch nugetVersion.Value).ToString()) else "0.0.0-local"), "((\\d+\\.?)+)(.*)", "$1-local")
    packageDir <- packagePushDir
    printfn "New version is %s \n" version
)

Target.Create "PrepareSources" (fun _ ->
    printfn "Creating a sources copy..."
    let sourcesDir = Path.Combine(buildDir, "src")
    ensureDirectory sourcesDir
    CleanDir sourcesDir
    
    Directory.GetDirectories "."
        |> Seq.filter (fun (dir:string) -> not (Seq.isEmpty (filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(dir))))) 
        |> Seq.iter (fun (dir:string) ->
        
                filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(dir))
                |> Seq.iter
                    (fun (file:FileInfo) -> 
                        let projectDir = Path.GetDirectoryName(file.FullName)
                        CleanDir (Path.Combine(projectDir, "bin")))

                let fullDir = Path.GetFullPath(dir)
                let destinationDir = Path.Combine(sourcesDir, Path.GetFileName(fullDir), ".")                
                CopyDir destinationDir fullDir (fun _ -> true))

    filesInDirMatching "*.sln" (new DirectoryInfo("."))
        |> Seq.iter (fun (file:FileInfo) -> CopyFile sourcesDir file.FullName)
    filesInDirMatching "*.props" (new DirectoryInfo("."))
        |> Seq.iter (fun (file:FileInfo) -> CopyFile sourcesDir file.FullName)
       

    let projects = filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
    projects 
    |> Seq.iter (fun (file:FileInfo) -> 
        let projectDir = Path.GetDirectoryName(file.FullName)
        CleanDir (Path.Combine(projectDir, "obj"))
        RegexReplaceInFileWithEncoding  "<Version>(.*)</Version>" (sprintf "<Version>%s</Version>" version) Text.Encoding.UTF8 file.FullName) 
)

"SetVersion" ?=> "PrepareSources"

Target.Create "Restore" (fun _ ->
    printfn "Restoring packages..."
    let sourcesDir = Path.Combine(buildDir, "src")  
    filesInDirMatching "*.sln" (new DirectoryInfo(sourcesDir))
    |> Seq.iter
        (fun (file:FileInfo) ->
            let setParams defaults = { 
                defaults with
                    Verbosity = Some(Minimal)
                    Targets = ["Restore"]
                    RestorePackagesFlag = true
                    Properties = 
                    [
                        "Optimize", "True"
                        "DebugSymbols", "True"
                        "Configuration", "Release"                        
                    ]
            }
            build setParams file.FullName)        
)

"PrepareSources" ==> "Restore"

Target.Create  "Build" (fun _ ->
    printfn "Build..."
    let sourcesDir = Path.Combine(buildDir, "src")  
    Seq.iter
        (fun (file:FileInfo) ->
            let setParams defaults = { 
                defaults with
                    Verbosity = Some(Minimal)
                    Targets = ["Restore"; "Build"]
                    RestorePackagesFlag = true
                    Properties = 
                    [
                        "Optimize", "True"
                        "DebugSymbols", "True"
                        "Configuration", "Release"                        
                    ]
            }
            build setParams file.FullName)
        (filesInDirMatching "*.sln" (new DirectoryInfo(sourcesDir)))
)

"Restore" ==> "Build"

Target.Create "Nuget" (fun _ ->
    printfn "Packing nuget..."
    let sourcesDir = Path.Combine(buildDir, "src")  
    filesInDirMatching "*.sln" (new DirectoryInfo(sourcesDir))
    |> Seq.iter
        (fun (file:FileInfo) ->
            let setParams defaults = { 
                defaults with
                    Verbosity = Some(Minimal)
                    Targets = ["Pack"]
                    RestorePackagesFlag = true
                    Properties = 
                    [
                        "Optimize", "True"
                        "DebugSymbols", "True"
                        "Configuration", "Release"                        
                    ]
            }
            build setParams file.FullName)

    CleanDir packageDir
    filesInDirMatchingRecursive "*.nupkg" (new DirectoryInfo(sourcesDir))
    |> Seq.iter
        (fun (file:FileInfo) ->
                printfn "%s" (Path.GetFileName file.FullName)
                CopyFile packageDir file.FullName)

    // workaround of https://github.com/NuGet/Home/issues/4360
    filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
    |> Seq.filter (fun (file:FileInfo) -> Regex.IsMatch(File.ReadAllText(file.FullName), "<IsTool>true</IsTool>", (RegexOptions.CultureInvariant ||| RegexOptions.IgnoreCase)))
    |> Seq.iter (fun (file:FileInfo) -> 
        printfn "Repacking tool: %s\n" (Path.GetFileName file.FullName)
        let projectDir = Path.GetDirectoryName file.FullName
        let nuspecName = filesInDirMatchingRecursive "*.nuspec" (new DirectoryInfo(projectDir)) |> Seq.head
        let nuspec = new XmlDocument()
        nuspec.LoadXml(File.ReadAllText(nuspecName.FullName))
        let namespaceManager = new XmlNamespaceManager(nuspec.NameTable);
        namespaceManager.AddNamespace("n", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd");
        let filesNode = nuspec.SelectSingleNode("/n:package/n:files", namespaceManager)
        filesNode.InnerXml <- ""
        let buildFilesPath = Path.Combine(projectDir, "bin", "Release")
        
        filesInDirMatchingRecursive "*.*" (new DirectoryInfo(buildFilesPath))
        |> Seq.filter (fun (file:FileInfo) -> not(Regex.IsMatch(file.FullName, "\\.nupkg$")))
        |> Seq.iter (fun (file:FileInfo) -> 
            let fileElement = filesNode.AppendChild(nuspec.CreateElement("file", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd")) :?> XmlElement
            let relativeFileName = file.FullName.Substring(buildFilesPath.Length)
            fileElement.SetAttribute("src", file.FullName);
            fileElement.SetAttribute("target", (sprintf "tools%s" relativeFileName))
        )
        (
            use stream = File.Create(nuspecName.FullName)
            nuspec.Save(stream)     
            stream.Dispose()
        )
        ExecProcess (fun info ->
            info.FileName <- "nuget.exe";
            info.Arguments <- sprintf "pack \"%s\" -OutputDirectory \"%s\"" nuspecName.FullName packageDir)
            (TimeSpan.FromMinutes 30.0)
            |> ignore       
    )
)

"Build" ==> "Nuget"


Target.Create "CleanDockerImages" (fun _ ->
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

Target.Create "PushLocalPackages" (fun _ ->
    pushPackage (Path.Combine(packagePushDir, "*.nupkg"))
)

"Nuget" ?=> "PushLocalPackages"

Target.Create "RePushLocalPackages" (fun _ ->
    Directory.GetFiles(packagePushDir)
        |> Seq.filter (fun f -> Path.GetExtension(f) = ".nupkg")
        |> Seq.iter pushPackage
)

Target.Create "FinalPushLocalPackages" (fun _ -> ())
"SetVersion" ==> "FinalPushLocalPackages"
"Nuget" ==> "FinalPushLocalPackages"
"PushLocalPackages" ==> "FinalPushLocalPackages"

Target.Create "RestoreThirdPartyPackages" (fun _ ->
    printfn "Restoring packages"
    
    let sourcesDir = Path.Combine(buildDir, "src") 
    ensureDirectory packageThirdPartyDir
    CleanDir packageThirdPartyDir
    let packageCache = SettingsUtility.GetGlobalPackagesFolder(NullSettings.Instance)
    let packages = 
        LocalFolderUtility.GetPackagesV3(packageCache, NullLogger.Instance)
    let packageGroups = packages
                            |> Seq.groupBy (fun (p:LocalPackageInfo) -> p.Identity.Id.ToLower())
                            |> dict
                            |> Dictionary<string, seq<LocalPackageInfo>>

    let directPackages = filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
                            |> Seq.map(fun (file:FileInfo) -> new Project(file.FullName, null, null, ProjectCollection.GlobalProjectCollection, ProjectLoadSettings.IgnoreMissingImports))
                            |> Seq.collect (fun proj -> proj.ItemsIgnoringCondition |> Seq.map (fun item -> (proj, item)))
                            |> Seq.filter (fun (_, item) -> item.ItemType = "PackageReference")
                            |> Seq.map (fun (proj, item) -> ((item.Xml.Include), (item.Metadata |> Seq.filter (fun d -> d.Name = "Version") |> Seq.tryHead |> (fun i -> if i.IsSome then i.Value.EvaluatedValue else null)), proj))
                            |> Seq.filter (fun (_, version, _) -> version <> null)
                            |> Seq.map (fun (id, version, _) -> 
                                // printfn "%s: %s %s" (Path.GetFileName proj.FullPath) id version  
                                (id, version)
                            )
                            |> Seq.distinct
                            |> Seq.sortBy (fun (id, _) -> id)
                            |> Seq.map (fun (id, version) -> 
                                let (success, list) = packageGroups.TryGetValue(id.ToLower().Trim())
                                if success then
                                    list
                                    |> Seq.filter (fun (p:LocalPackageInfo) -> VersionRange.Parse(version).Satisfies(p.Identity.Version))
                                    |> Seq.sortBy (fun (p:LocalPackageInfo) -> p.Identity.Version)
                                    |> Seq.tryHead
                                else
                                    printfn "!!!!Package %s was not found" (id.ToLower().Trim())
                                    None)
                            |> Seq.filter (fun (p:LocalPackageInfo option) -> p.IsSome)
                            |> Seq.map (fun (p:LocalPackageInfo option) -> p.Value)
                            |> Seq.distinct
                            |> Seq.map (fun (p:LocalPackageInfo) -> p.Identity, p)
                            |> dict
   
    let getDirectDependecies (packages : IDictionary<Core.PackageIdentity, LocalPackageInfo>) = 
        packages.Values
            |> Seq.collect(fun (p:LocalPackageInfo) -> p.Nuspec.GetDependencyGroups())
            |> Seq.collect(fun (dg:PackageDependencyGroup) -> dg.Packages)
            |> Seq.distinct
            |> Seq.map (fun (d: Core.PackageDependency) ->
                let (success, list) = packageGroups.TryGetValue(d.Id.ToLower())
                if success then
                    list 
                        |> Seq.filter (fun (p:LocalPackageInfo) -> (d.VersionRange.Satisfies(p.Identity.Version)))
                        |> Seq.sortBy (fun (p:LocalPackageInfo) -> p.Identity.Version)
                        |> Seq.tryHead 
                        |> (fun (p:LocalPackageInfo option) -> if p.IsSome then p.Value :> Object else d :> Object)
                else d :> Object
                )
            |> Seq.map (fun arg -> 
                match arg with
                | :? LocalPackageInfo as p -> p
                | :? Core.PackageDependency as d -> 
                    printfn "Package requirement %s %s was not found, installing" d.Id (d.VersionRange.ToString())
                    ExecProcess (fun info ->
                            info.FileName <- "nuget.exe";
                            info.Arguments <- sprintf "install %s -Version %s -Prerelease" d.Id (d.VersionRange.MinVersion.ToString()))
                            (TimeSpan.FromMinutes 30.0)
                            |> ignore
                    let newPackage = 
                        LocalFolderUtility.GetPackagesV3(packageCache, NullLogger.Instance)
                        |> Seq.filter(fun p -> p.Identity.Id.ToLower() = d.Id.ToLower() && p.Identity.Version = (NuGetVersion.Parse(d.VersionRange.MinVersion.ToString())))
                        |> Seq.tryHead
                    
                    if newPackage.IsNone then failwith  (sprintf "package install of %s %s failed" d.Id (d.VersionRange.ToString()))
                    
                    if not(packageGroups.ContainsKey(newPackage.Value.Identity.Id.ToLower()))
                    then packageGroups.Add(newPackage.Value.Identity.Id.ToLower(), [newPackage.Value]) 
                    else packageGroups.[newPackage.Value.Identity.Id.ToLower()] <- (packageGroups.[newPackage.Value.Identity.Id.ToLower()] |> Seq.append [newPackage.Value])                   
                    newPackage.Value
                | _ -> failwith "strange")
            |> Seq.cast<LocalPackageInfo>
            |> Seq.distinct
            |> Seq.filter(fun (p:LocalPackageInfo) -> not (packages.ContainsKey(p.Identity)))  

    let rec getPackagesWithDependencies (_packages : IDictionary<Core.PackageIdentity, LocalPackageInfo>) = 
        let _directDependencies = getDirectDependecies _packages
        if _directDependencies |> Seq.isEmpty then
            _packages
        else
            _packages.Values 
                |> Seq.append _directDependencies 
                |> Seq.map (fun (p:LocalPackageInfo) -> p.Identity, p)
                |> dict                
                |> getPackagesWithDependencies

    printfn "%d start packages"  (directPackages |> Seq.length)
    
    let dependecies = 
        getPackagesWithDependencies directPackages

    let filteredDependencies =
        dependecies.Values
        |> Seq.groupBy(fun (p:LocalPackageInfo) -> p.Identity.Id)
        |> Seq.map (fun (_, list) -> list |> Seq.sortBy(fun p -> p.Identity.Version) |> Seq.last)
        
    filteredDependencies
    |> Seq.sortBy(fun (p:LocalPackageInfo) -> p.Identity.Id)
    |> Seq.iter (fun (p:LocalPackageInfo) ->
        CopyFile packageThirdPartyDir p.Path        
    )

    printfn "total %d third party packages"  (filteredDependencies |> Seq.length)      
)

"PrepareSources" ==> "RestoreThirdPartyPackages"

Target.Create "PushThirdPartyPackages" (fun _ ->
    pushPackage (Path.Combine(packageThirdPartyDir, "*.nupkg"))
)

"PushThirdPartyPackages" ?=> "PushLocalPackages"
"RestoreThirdPartyPackages" ?=> "PushThirdPartyPackages"

Target.Create "RePushThirdPartyPackages" (fun _ ->
    filesInDirMatchingRecursive "*.nupkg" (new DirectoryInfo(packageThirdPartyDir))
        |> Seq.map (fun (file:FileInfo) -> file.FullName)
        |> Seq.iter pushPackage
)

Target.Create "FinalPushThirdPartyPackages" (fun _ -> ())
"RestoreThirdPartyPackages" ==> "FinalPushThirdPartyPackages"
"PushThirdPartyPackages" ==> "FinalPushThirdPartyPackages"

Target.Create "FinalPushAllPackages" (fun _ -> ())
"FinalPushThirdPartyPackages" ==> "FinalPushAllPackages"
"FinalPushLocalPackages" ==> "FinalPushAllPackages"

// FINISHED Base part
// Starting specific part

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
        build setParams projectPath

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

        let copyThirdPartyPackage (f: FileInfo) =
            if (Path.GetExtension(f.FullName) = ".nupkg") then
                if not (File.Exists (Path.Combine [|(Path.GetFullPath("./packageOut/")); f.Name|])) then
                    Copy packageCacheDir [|f.FullName|]

        (new DirectoryInfo(packageThirdPartyDir)).GetFiles("*", SearchOption.TopDirectoryOnly)
        |> Seq.iter copyThirdPartyPackage
        

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
    (*
    if not (directoryExists "./packages/Npm.js") then
        "Npm.js" |> RestorePackageId(fun p-> 
            { p with
                OutputPath = "./packages"
                ExcludeVersion = true})
    
    Fake.FileHelper.Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env-local" "./Docker/ClusterKitMonitoring/clusterkit-web/.env"
    Fake.FileHelper.Rename "./Docker/ClusterKitMonitoring/clusterkit-web/.env" "./Docker/ClusterKitMonitoring/clusterkit-web/.env-build"
    *)
    (*
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
    *)
)


"DockerBase" ?=> "CleanDockerImages"
"DockerContainers" ?=> "CleanDockerImages"
"DockerContainers" ?=> "PrepareSources"
"DockerBase" ?=> "DockerContainers"

// prepares docker images
Target.Create  "FinalBuildDocker" (fun _ -> ())
"DockerBase" ==> "FinalBuildDocker"
"DockerContainers" ==> "FinalBuildDocker"
"CleanDockerImages" ==> "FinalBuildDocker"

Target.RunOrDefault "Nuget"
