#r "../packages/FAKE/tools/FakeLib.dll" // include Fake lib

open Fake
open System
open System.IO
open System.Xml
open System.Linq

module BuildTools =
    // Properties

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