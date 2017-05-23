// #r @"BuildScripts/FakeLib.dll" // include Fake lib
// #r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils // include budle of build utils
#I @"packages/FAKE/tools"
#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.IO

let buildDir = Path.GetFullPath("./build")
let packageOutDir = Path.GetFullPath("./packageOut")
let packagePushDir = Path.GetFullPath("./packagePush")
let envVersion = environVar "version"
let packageDir = if envVersion <> null then packagePushDir else packageOutDir
let version = if envVersion <> null then envVersion else "0.0.0-local"

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
    CleanDirs [|
        packageDir
        buildDir        
        |]
    )

Target "Build" (fun _ ->
    trace "Build..."
    let projects = filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo("."))
    Seq.iter (fun (file:FileInfo) -> 
        trace file.FullName
        let setParams defaults = { 
            defaults with
                Verbosity = Some(Minimal)
                Targets = ["Build"]
                RestorePackagesFlag = true
                Properties = 
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", "Release"
                    "OutputPath", Path.Combine(buildDir, "bin", Path.GetFileNameWithoutExtension(file.Name))
                ]
        }
        build setParams file.FullName
    ) projects
)

//---------------------------------------
//Nuget creation
//---------------------------------------
 
Target "Nuget" (fun _ ->
    trace "Creating a sources copy..."
    let sourcesDir = Path.Combine(buildDir, "src")
    ensureDirectory sourcesDir
    CleanDir sourcesDir
    Seq.iter 
        (fun (dir:string) -> 
                let fullDir = Path.GetFullPath(dir)
                let destinationDir = Path.Combine(sourcesDir, Path.GetFileName(fullDir), ".")
                CleanDir (Path.Combine(fullDir, "bin"))
                CopyDir destinationDir fullDir (fun _ -> true))
        (Seq.filter 
            (fun (dir:string) -> not (Seq.isEmpty (filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(dir))))) 
            (Directory.GetDirectories(".")))
    Seq.iter
        (fun (file:FileInfo) ->
                CopyFile sourcesDir file.FullName)
        (filesInDirMatching "*.sln" (new DirectoryInfo(".")))
        

    let projects = filesInDirMatchingRecursive "*.csproj" (new DirectoryInfo(sourcesDir))
    Seq.iter (fun (file:FileInfo) -> 
        RegexReplaceInFileWithEncoding  "<Version>(.*)</Version>" version Text.Encoding.UTF8 file.FullName 
    ) projects

    Seq.iter
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
        (filesInDirMatching "*.sln" (new DirectoryInfo(sourcesDir)))
    CleanDir packageDir
    Seq.iter
        (fun (file:FileInfo) ->
                CopyFile packageDir file.FullName)
        (filesInDirMatching "*.nupkg" (new DirectoryInfo(sourcesDir)))
)
