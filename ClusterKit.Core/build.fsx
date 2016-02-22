#r "../packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake
open System
open System.IO
open System.Xml

// Properties
let buildDir = "./build"
let packageDir = "./packageOut"
let ver = environVar "version"

let installer (projFile : string) =
    let dir = Path.GetDirectoryName(Path.GetFullPath(projFile))
    trace dir
    let parts = dir.Split([|'/';'\\'|], StringSplitOptions.RemoveEmptyEntries)
    let projName = parts.[parts.Length - 1]

    let buildTmpDir = [buildDir; "tmp"; projName] |> String.concat "/";
    let outputDir = [buildDir; "clean"; projName] |> String.concat "/"

    // cleaning
    if Directory.Exists(buildTmpDir) then Directory.Delete(buildTmpDir, true)
    if Directory.Exists(outputDir) then Directory.Delete(outputDir, true)

    // building project
    MSBuildRelease
        buildTmpDir
        "Build"
        [projFile]
         |> Log "AppBuild: "

    // preparing output
    Directory.CreateDirectory(outputDir) |> ignore
    let files = Directory.GetFiles(buildTmpDir,  [projName; ".*"] |> String.concat "")
    for file in files do
        trace  ([buildTmpDir; Path.GetFileName(file)] |> String.concat "/")
        File.Copy(
            [buildTmpDir; Path.GetFileName(file)] |> String.concat "/",
            [outputDir; Path.GetFileName(file)] |> String.concat "/"
        )

    // creating nuspec file
    let nuspecData = new XmlDocument()
    nuspecData.Load((dir + "/" + projName + ".nuspec"))
    let metadata = nuspecData.DocumentElement.SelectSingleNode("/package/metadata")
    metadata.SelectSingleNode("id").InnerText <- projName
    metadata.SelectSingleNode("version").InnerText <- ver
    metadata.SelectSingleNode("title").InnerText <- projName
    metadata.SelectSingleNode("authors").InnerText <- "ClusterKit Team"  // todo: @m_kantarovsky publish correct data
    metadata.SelectSingleNode("owners").InnerText <- "ClusterKit Team"  // todo: @m_kantarovsky publish correct data
    metadata.SelectSingleNode("description").InnerText <- "ClusterKit lib" // todo: @m_kantarovsky publish correct data
    let dependenciesRootElement = metadata.AppendChild(nuspecData.CreateElement("dependencies"))
    let dependenciesDoc = new XmlDocument()
    dependenciesDoc.Load(dir + "/packages.config")

    for dependency in dependenciesDoc.DocumentElement.SelectNodes("/packages/package") do
        let dependencyElement = dependenciesRootElement.AppendChild(nuspecData.CreateElement("dependency"))
        dependencyElement.Attributes.Append(nuspecData.CreateAttribute("id")).Value <- dependency.Attributes.["id"].Value
        dependencyElement.Attributes.Append(nuspecData.CreateAttribute("version")).Value <- dependency.Attributes.["version"].Value

    let nuspecFile = outputDir + "/" + projName + ".nuspec"
    nuspecData.Save(nuspecFile)

    let nugetParams defaults : NuGetParams =
        {   defaults  with
                 WorkingDir = outputDir
                 OutputPath = packageDir
                 Version = ver
        }

    NuGetPackDirectly nugetParams  nuspecFile

    // cleaning
    if Directory.Exists(buildTmpDir) then Directory.Delete(buildTmpDir, true)
    if Directory.Exists(outputDir) then Directory.Delete(outputDir, true)

//    File.Copy(
//        [dir; (projName + ".nuspec")] |> String.concat "/",
//        [outputDir; (projName + ".nuspec")] |> String.concat "/"
//    )

//            "./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj"
//            "./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj"

Target "BuildApp" (fun _ ->
      installer("./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj")
)

Target "Deploy" (fun _ ->
    trace "Heavy deploy action"
)

"BuildApp"
   ==> "Deploy"

Run "Deploy"