// #r @"BuildScripts/FakeLib.dll" // include Fake lib
// #r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils // include budle of build utils
#I @"packages/FAKE/tools"
#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.IO
open System.Text.RegularExpressions
open System.Xml

let testPackageName = "ClusterKit.Core"
let buildDir = Path.GetFullPath("./build")
let packageOutDir = Path.GetFullPath("./packageOut")
let packagePushDir = Path.GetFullPath("./packagePush")
let packageThirdPartyDir = Path.GetFullPath("./packageThirdPartyDir")
let envVersion = environVar "version"
let packageDir = if envVersion <> null then packagePushDir else packageOutDir
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
    Directory.GetFiles(packagePushDir)
        |> Seq.filter (hasExt ".nupkg")
        |> Seq.iter pushPackage
)
"Nuget" ?=> "PushLocalPackages"

Target "FinalPushLocalPackages" (fun _ -> ())
"SetVersion" ==> "FinalPushLocalPackages"
"Nuget" ==> "FinalPushLocalPackages"
"PushLocalPackages" ==> "FinalPushLocalPackages"

Target "RestoreThirdPartyPackages" (fun _ ->
    ensureDirectory packageThirdPartyDir
    CleanDir packageThirdPartyDir
    let sourcesDir = Path.Combine(buildDir, "src") 
    filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
    |> Seq.collect (fun (file:FileInfo) -> 
            let xml = File.ReadAllText(file.FullName) |> XMLDoc
            xml.SelectNodes("/Project/ItemGroup/PackageReference")
            |> Seq.cast<System.Xml.XmlElement>
            |> Seq.map (fun (node:System.Xml.XmlElement) -> ((node.GetAttribute("Include"), (node.GetAttribute("Version"))))))
    |> Seq.distinct
    |> Seq.sortBy (fun (id, version) -> id)
    |> Seq.iter (fun (id, version) -> 
        tracef "%s %s\n" id version
        ExecProcess (fun info ->
            info.FileName <- "nuget.exe";
            info.Arguments <- sprintf "install %s -Version %s -OutputDirectory %s" id version packageThirdPartyDir)
            (TimeSpan.FromMinutes 30.0)
            |> ignore
    )
)

"PrepareSources" ==> "RestoreThirdPartyPackages"

Target "PushThirdPartyPackages" (fun _ ->
    filesInDirMatchingRecursive "*.nupkg" (new DirectoryInfo(packageThirdPartyDir))
        |> Seq.map (fun (file:FileInfo) -> file.FullName)
        |> Seq.iter pushPackage
)

"PushThirdPartyPackages" <=? "PushLocalPackages"

"RestoreThirdPartyPackages" ?=> "PushThirdPartyPackages"

Target "FinalPushThirdPartyPackages" (fun _ -> ())
"RestoreThirdPartyPackages" ==> "FinalPushThirdPartyPackages"
"PushThirdPartyPackages" ==> "FinalPushThirdPartyPackages"

Target "FinalPushAllPackages" (fun _ -> ())
"FinalPushThirdPartyPackages" ==> "FinalPushAllPackages"
"FinalPushLocalPackages" ==> "FinalPushAllPackages"

