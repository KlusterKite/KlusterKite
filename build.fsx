#r "./packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake
open System
open System.IO
open System.Xml
open System.Linq

// Properties
let buildDir = "./build"
let packageDir = "./packageOut"
let ver = environVar "version"

// package building method
let installer (projFile : string, internalDependencies : string[]) =
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

    // adding dependencies

    let dependenciesRootElement = metadata.AppendChild(nuspecData.CreateElement("dependencies"))
    let dependenciesDoc = new XmlDocument()
    dependenciesDoc.Load(dir + "/packages.config")

    for dependency in dependenciesDoc.DocumentElement.SelectNodes("/packages/package") do
        if not(internalDependencies.Contains(dependency.Attributes.["id"].Value)) then
            let dependencyElement = dependenciesRootElement.AppendChild(nuspecData.CreateElement("dependency"))
            dependencyElement.Attributes.Append(nuspecData.CreateAttribute("id")).Value <- dependency.Attributes.["id"].Value
            dependencyElement.Attributes.Append(nuspecData.CreateAttribute("version")).Value <- dependency.Attributes.["version"].Value

    for dependency in internalDependencies do
        let dependencyElement = dependenciesRootElement.AppendChild(nuspecData.CreateElement("dependency"))
        dependencyElement.Attributes.Append(nuspecData.CreateAttribute("id")).Value <- dependency
        dependencyElement.Attributes.Append(nuspecData.CreateAttribute("version")).Value <- ver

    let filesRootElement = nuspecData.DocumentElement.SelectSingleNode("/package").AppendChild(nuspecData.CreateElement("files"))
    for file in Directory.GetFiles(outputDir) do
        let fileElement = filesRootElement.AppendChild(nuspecData.CreateElement("file"))
        fileElement.Attributes.Append(nuspecData.CreateAttribute("src")).Value <- Path.GetFileName(file)
        fileElement.Attributes.Append(nuspecData.CreateAttribute("target")).Value <- ("./lib/" + Path.GetFileName(file))

    let nuspecFile = outputDir + "/" + projName + ".nuspec"
    nuspecData.Save(nuspecFile)

    // generating package
    if not (Directory.Exists(packageDir)) then Directory.CreateDirectory(packageDir) |> ignore
    let nugetParams defaults : NuGetParams =
        {   defaults  with
                 WorkingDir = outputDir
                 OutputPath = packageDir
                 Version = ver
        }

    NuGetPackDirectly nugetParams  nuspecFile

//            "./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj"
//            "./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj"

Target "BuildApp" (fun _ ->
    if Directory.Exists(packageDir) then Directory.Delete(packageDir, true)
    installer("./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj", ([||]))
    installer("./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ([|"ClusterKit.Core"|]))
    installer("./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ([|"ClusterKit.Core"|]))
    installer("./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ([|"ClusterKit.Core"|]))
    installer("./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    installer("./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    installer("./ClusterKit.Web/ClusterKit.Web.SignalR/ClusterKit.Web.SignalR.csproj", ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))

    if Directory.Exists(buildDir) then Directory.Delete(buildDir, true)
)

Target "Deploy" (fun _ ->
    trace "Heavy deploy action"
)

"BuildApp"
   ==> "Deploy"

Run "Deploy"