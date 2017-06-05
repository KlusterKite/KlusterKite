 // #r @"BuildScripts/FakeLib.dll" // include Fake lib
// #r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils // include budle of build utils
#I @"packages/FAKE/tools"
#r @"packages/FAKE/tools/FakeLib.dll"
#r @"packages/NuGet.Common/lib/net45/NuGet.Common.dll"
#r @"packages/NuGet.Configuration/lib/net45/NuGet.Configuration.dll"
#r @"packages/NuGet.Packaging.Core/lib/net45/NuGet.Packaging.Core.dll"
#r @"packages/NuGet.Packaging/lib/net45/NuGet.Packaging.dll"
#r @"packages/NuGet.Frameworks/lib/net45/NuGet.Frameworks.dll"
#r @"packages/NuGet.Versioning/lib/net45/NuGet.Versioning.dll"
#r @"packages/NuGet.Packaging.Core.Types/lib/net45/NuGet.Packaging.Core.Types.dll"
#r @"packages/NuGet.Protocol.Core.Types/lib/net45/NuGet.Protocol.Core.Types.dll"
#r @"packages/NuGet.Protocol.Core.v3/lib/net45/NuGet.Protocol.Core.v3.dll"
#r @"packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#r @"System.Net.Http"
#r @"System.Xml.Linq"
#r @"System.IO.Compression"

open Fake
open System
open System.IO
open System.Text.RegularExpressions
open System.Xml

open System.Collections.Generic;

open NuGet.Configuration;
open NuGet.Common;
open NuGet.Protocol;
open NuGet.Versioning;
open NuGet.Packaging;

let testPackageName = "ClusterKit.Core"
let buildDir = Path.GetFullPath("./build")
let packageOutDir = Path.GetFullPath("./packageOut")
let packagePushDir = Path.GetFullPath("./packagePush")
let packageThirdPartyDir = Path.GetFullPath("./packageThirdPartyDir")
let envVersion = environVar "version"
let mutable packageDir = packageOutDir
let mutable version = if envVersion <> null then envVersion else "0.0.0-local"

let currentTarget = getBuildParam "target"

let buildDocker (containerName:string) (path:string) =
    if (ExecProcess (fun info ->
        info.FileName <- "docker"
        info.Arguments <- (sprintf "build -t %s:latest %s" containerName path)
    )  (TimeSpan.FromDays 2.0) <> 0) then
        failwithf "Error while building %s" path

let pushPackage (package:string) =
    let localPath = Fake.FileSystemHelper.currentDirectory
    let packageLocal = package.Replace(localPath, ".")
    ExecProcess (fun info ->
        info.FileName <- "nuget.exe";
        info.Arguments <- sprintf "push %s -Source %s -ApiKey %s" packageLocal "http://docker:81/" "ClusterKit")
        (TimeSpan.FromMinutes 30.0)
        |> ignore

Target "Clean" (fun _ ->
    trace "PreClean..."
    CleanDir buildDir
)

// switches nuget and build version from init one, to latest posible on docker nuget server
Target "SetVersion" (fun _ ->
    let nugetVersion = Fake.NuGetVersion.getLastNuGetVersion "http://docker:81" testPackageName
    if nugetVersion.IsSome then tracef "Current version is %s \n" (nugetVersion.ToString()) else trace "Repository is empty"
    version <- Regex.Replace((if nugetVersion.IsSome then ((Fake.NuGetVersion.IncPatch nugetVersion.Value).ToString()) else "0.0.0-local"), "((\\d+\\.?)+)(.*)", "$1-local")
    packageDir <- packagePushDir
    tracef "New version is %s \n" version
)    

Target "PrepareSources" (fun _ ->
    trace "Creating a sources copy..."
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
       

    let projects = filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
    projects 
    |> Seq.iter (fun (file:FileInfo) -> 
        let projectDir = Path.GetDirectoryName(file.FullName)
        CleanDir (Path.Combine(projectDir, "obj"))
        RegexReplaceInFileWithEncoding  "<Version>(.*)</Version>" (sprintf "<Version>%s</Version>" version) Text.Encoding.UTF8 file.FullName) 
)

"SetVersion" ?=> "PrepareSources"

Target "Restore" (fun _ ->
    trace "Restoring packages..."
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

Target "Build" (fun _ ->
    trace "Build..."
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

Target "Nuget" (fun _ ->
    trace "Packing nuget..."
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
                trace (Path.GetFileName file.FullName)
                CopyFile packageDir file.FullName)

    // workaround of https://github.com/NuGet/Home/issues/4360
    filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
    |> Seq.filter (fun (file:FileInfo) -> Regex.IsMatch(File.ReadAllText(file.FullName), "<IsTool>true</IsTool>", (RegexOptions.CultureInvariant ||| RegexOptions.IgnoreCase)))
    |> Seq.iter (fun (file:FileInfo) -> 
        tracef "Repacking tool: %s\n" (Path.GetFileName file.FullName)
        let projectDir = Path.GetDirectoryName file.FullName
        let nuspecName = filesInDirMatchingRecursive "*.nuspec" (new DirectoryInfo(projectDir)) |> Seq.head
        let nuspec = XMLDoc (File.ReadAllText(nuspecName.FullName))
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
        
        nuspec.Save(nuspecName.FullName)     
        ExecProcess (fun info ->
            info.FileName <- "nuget.exe";
            info.Arguments <- sprintf "pack \"%s\" -OutputDirectory \"%s\"" nuspecName.FullName packageDir)
            (TimeSpan.FromMinutes 30.0)
            |> ignore       
    )
)

"Build" ==> "Nuget"


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

Target "PushLocalPackages" (fun _ ->
    pushPackage (Path.Combine(packagePushDir, "*.nupkg"))
)

"Nuget" ?=> "PushLocalPackages"

Target "RePushLocalPackages" (fun _ ->
    Directory.GetFiles(packagePushDir)
        |> Seq.filter (hasExt ".nupkg")
        |> Seq.iter pushPackage
)

Target "FinalPushLocalPackages" (fun _ -> ())
"SetVersion" ==> "FinalPushLocalPackages"
"Nuget" ==> "FinalPushLocalPackages"
"PushLocalPackages" ==> "FinalPushLocalPackages"

Target "RestoreThirdPartyPackages" (fun _ ->
    trace "Restoring packages"
    ensureDirectory packageThirdPartyDir
    CleanDir packageThirdPartyDir
    let sourcesDir = Path.Combine(buildDir, "src") 

    let packageCache = SettingsUtility.GetGlobalPackagesFolder(NullSettings.Instance)
    let packages = 
        LocalFolderUtility.GetPackagesV3(packageCache, NullLogger.Instance)

    let packageGroups = packages
                            |> Seq.groupBy (fun (p:LocalPackageInfo) -> p.Identity.Id.ToLower())
                            |> dict
                            |> Dictionary<string, seq<LocalPackageInfo>>
    
    let directPackages = filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
                            |> Seq.collect (fun (file:FileInfo) -> 
                                let xml = File.ReadAllText(file.FullName) |> XMLDoc
                                xml.SelectNodes("/Project/ItemGroup/PackageReference")
                                |> Seq.cast<System.Xml.XmlElement>
                                |> Seq.map (fun (node:System.Xml.XmlElement) -> ((node.GetAttribute("Include"), (node.GetAttribute("Version"))))))
                            |> Seq.distinct
                            |> Seq.sortBy (fun (id, version) -> id)
                            |> Seq.map (fun (id, version) -> 
                                let (success, list) = packageGroups.TryGetValue(id.ToLower())
                                if success then
                                    list
                                    |> Seq.filter (fun (p:LocalPackageInfo) -> p.Identity.Version = NuGetVersion.Parse(version)) 
                                    |> Seq.tryHead
                                else None)
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
                    traceFAKE "Package requirement %s %s was not found, installing" d.Id (d.VersionRange.ToString())
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
        // tracef "found %d new dependencies \n"  (_directDependencies |> Seq.length)
        if _directDependencies |> Seq.isEmpty then
            _packages
        else
            _packages.Values 
                |> Seq.append _directDependencies 
                |> Seq.map (fun (p:LocalPackageInfo) -> p.Identity, p)
                |> dict                
                |> getPackagesWithDependencies

    tracef "%d start packages\n"  (directPackages |> Seq.length)
    let dependecies = 
        getPackagesWithDependencies directPackages

    let filteredDependencies =
        dependecies.Values
        |> Seq.groupBy(fun (p:LocalPackageInfo) -> p.Identity.Id)
        |> Seq.map (fun (key, list) -> list |> Seq.sortBy(fun p -> p.Identity.Version) |> Seq.last)
        
    
    filteredDependencies
    |> Seq.sortBy(fun (p:LocalPackageInfo) -> p.Identity.Id)
    |> Seq.iter (fun (p:LocalPackageInfo) ->
        CopyFile packageThirdPartyDir p.Path        
    )

    tracef "total %d third party packages \n"  (filteredDependencies |> Seq.length)
)

"PrepareSources" ==> "RestoreThirdPartyPackages"

Target "PushThirdPartyPackages" (fun _ ->
    pushPackage (Path.Combine(packageThirdPartyDir, "*.nupkg"))
)

"PushThirdPartyPackages" ?=> "PushLocalPackages"
"RestoreThirdPartyPackages" ?=> "PushThirdPartyPackages"

Target "RePushThirdPartyPackages" (fun _ ->
    filesInDirMatchingRecursive "*.nupkg" (new DirectoryInfo(packageThirdPartyDir))
        |> Seq.map (fun (file:FileInfo) -> file.FullName)
        |> Seq.iter pushPackage
)

Target "FinalPushThirdPartyPackages" (fun _ -> ())
"RestoreThirdPartyPackages" ==> "FinalPushThirdPartyPackages"
"PushThirdPartyPackages" ==> "FinalPushThirdPartyPackages"

Target "FinalPushAllPackages" (fun _ -> ())
"FinalPushThirdPartyPackages" ==> "FinalPushAllPackages"
"FinalPushLocalPackages" ==> "FinalPushAllPackages"

