#load ".fake/build.fsx/loadDependencies.fsx"

open Fake.Core.Targets
open Fake.DotNet.Nuget

// switches nuget and build version from init one, to latest posible on docker nuget server
Target "SetVersion" (fun _ ->
    let nugetVersion = Fake.NuGetVersion.getLastNuGetVersion "http://docker:81" testPackageName
    if nugetVersion.IsSome then tracef "Current version is %s \n" (nugetVersion.ToString()) else trace "Repository is empty"
    version <- Regex.Replace((if nugetVersion.IsSome then ((Fake.NuGetVersion.IncPatch nugetVersion.Value).ToString()) else "0.0.0-local"), "((\\d+\\.?)+)(.*)", "$1-local")
    packageDir <- packagePushDir
    tracef "New version is %s \n" version
) 