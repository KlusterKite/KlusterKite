#r "./FakeLib.dll" // include Fake lib
#r @"./ClusterKit.Build.dll" // include budle of build utils
open Fake
open System
open System.IO
open System.Xml
open System.Linq
open System.Text.RegularExpressions

open  ClusterKit.Build

let buildDir = Path.GetFullPath("./build")
let packageDir = Path.GetFullPath("./packageOut")
let packagePushDir = Path.GetFullPath("./packagePush")
let ver = environVar "version"

let currentTarget = getBuildParam "target"

BuildUtils.Configure((if ver <> null then ver else "0.0.0-local"), buildDir, packageDir, "./packages")

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
        info.Arguments <- sprintf "push %s -Source %s -ApiKey %s" packageLocal "http://docker:81/" "ClusterKit")
        (TimeSpan.FromMinutes 30.0)
        |> ignore

// This target removes all temp and build result files
Target "Clean" (fun _ ->
    trace "PreClean..."
    Fake.FileHelper.CleanDirs [|
        BuildUtils.PackageOutputDirectory
        buildDir
        Path.Combine(buildDir, "tmp")
        Path.Combine(buildDir, "clean")
        |]
)

// perfoms global project compilation
Target "Build"  (fun _ ->
    BuildUtils.Build(BuildUtils.GetProjects());
)

// creates nuget package for every project
Target "CreateNuGet" (fun _ ->
    Fake.FileHelper.CleanDirs [|BuildUtils.PackageOutputDirectory|]
    BuildUtils.CreateNuget(BuildUtils.GetProjects());
)

// removes installed internal package from packages directory and restores them from latest build
Target "RefreshLocalDependencies" (fun _ ->
    BuildUtils.ReloadNuget(BuildUtils.GetProjects());
)

// runs all xunit tests
Target "Test" (fun _ ->
   
   
    let testAssemblies = (BuildUtils.GetProjects() 
        |> Seq.where (fun p -> p.ProjectType.HasFlag(ProjectDescription.EnProjectType.XUnitTests))
        |> Seq.map(fun project -> Path.Combine(project.TempBuildDirectory, project.ProjectName + ".dll")))

    let runnerLocation = (Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "packages")) 
        |> Seq.filter(fun d -> d.IndexOf("xunit.runner.console") > 0)
        |> Seq.sortByDescending (fun d -> d) 
        |> Seq.head)

    
    
    testAssemblies |> Fake.Testing.XUnit2.xUnit2 ( fun p -> {p with
                                                      ForceTeamCity = true;
                                                      ToolPath = Path.Combine(runnerLocation, "tools", "xunit.console.exe");
                                                      TimeOut = TimeSpan.FromHours(1.0);
                                                      Parallel =  Fake.Testing.XUnit2.ParallelMode.NoParallelization;} )
                                                      

    //BuildUtils.RunXUnitTest(projects);

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
                if not (File.Exists (Path.Combine [|packagePushDir; f.Name|])) then
                    pushPackage f.FullName

    Fake.FileHelper.recursively
            (fun d -> ())
            pushThirdPartyPackage
            (new DirectoryInfo(Path.GetFullPath("./packages")))

)

// sends prepared packages to docker nuget server
Target "PushLocalPackages" (fun _ ->
    Directory.GetFiles(packagePushDir)
        |> Seq.filter (hasExt ".nupkg")
        |> Seq.iter pushPackage
)

Target "CreateGlobalSolution"  (fun _ ->
    BuildUtils.CreateGlobalSolution(BuildUtils.GetProjects());
)

Target "SwitchToPackageRefs"  (fun _ ->
    BuildUtils.SwitchToPackageRefs(BuildUtils.GetProjects());
)

Target "SwitchToProjectRefs"  (fun _ ->
    BuildUtils.SwitchToProjectRefs(BuildUtils.GetProjects());
)

// switches nuget and build version from init one, to latest posible on docker nuget server
Target "SetVersion" (fun _ ->

    let packageName = BuildUtils.GetProjects() |> Seq.choose (fun p -> Some p.ProjectName) |> Seq.head

    let nugetVersion = Fake.NuGetVersion.getLastNuGetVersion "http://docker:81" packageName
    if nugetVersion.IsSome then tracef "Current version is %s \n" (nugetVersion.ToString()) else trace "Repository is empty"
    let version = Regex.Replace((if nugetVersion.IsSome then ((Fake.NuGetVersion.IncPatch nugetVersion.Value).ToString()) else "0.0.0-local"), "((\\d+\\.?)+)(.*)", "$1-local")
    tracef "New version is %s \n" version
    BuildUtils.Configure(version, buildDir, packagePushDir, "./packages")
)

// removes all installed packages and restores them (so this will remove obsolete packages)
Target "CleanPackageCache" (fun _ ->
    Directory.GetDirectories(Path.GetFullPath("./packages"))
        |> Seq.filter (fun d -> not("FAKE".Equals(d.Split(Path.DirectorySeparatorChar) |> Seq.last)))
        |> Seq.iter (fun d -> try Fake.FileUtils.rm_rf d with e -> tracefn "could not remove %s" d)
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
"Build" ?=> "CleanPackageCache"
"CreateNuGet" ?=> "RefreshLocalDependencies"

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

// builds local packages and sends them to local cluster nuget server
Target "FinalPushLocalPackages" (fun _ -> ())
"SetVersion" ==> "FinalPushLocalPackages"
"FinalCreateNuGet" ==> "FinalPushLocalPackages"
"PushLocalPackages" ==> "FinalPushLocalPackages"

// builds local packages and sends them to local cluster nuget server
Target "FinalPushAllPackages" (fun _ -> ())
"CleanPackageCache" ==> "FinalPushAllPackages"
"FinalPushLocalPackages" ==> "FinalPushAllPackages"
"PushThirdPartyPackages" ==> "FinalPushAllPackages"

// rebuilds current project and reinstall local dependent packages
Target "FinalRefreshLocalDependencies" (fun _ -> ())
"RefreshLocalDependencies" ==> "FinalRefreshLocalDependencies"
"FinalCreateNuGet" ==> "FinalRefreshLocalDependencies"
"CleanPackageCache" ==> "FinalRefreshLocalDependencies"