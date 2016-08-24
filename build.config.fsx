#r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils

open  ClusterKit.Build

let projects = [|
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Build/ClusterKit.Build.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.Rest/ClusterKit.Core.Rest.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.Data/ClusterKit.Core.Data.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"|]))

    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.Data.TestKit/ClusterKit.Core.Data.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Core.Data"|]))
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.EF/ClusterKit.Core.EF.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Core.Data"|]))
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.EF.TestKit/ClusterKit.Core.EF.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Core.Data"; "ClusterKit.Core.EF"|]))
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.EF.Npgsql/ClusterKit.Core.EF.Npgsql.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"; "ClusterKit.Core.Data"|]))

    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.Tests/ClusterKit.Core.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.Data"; "ClusterKit.Core.EF"; "ClusterKit.Core.EF.Npgsql"; "ClusterKit.Core.Rest"; "ClusterKit.Core.TestKit"|]))

    new ProjectDescription("ClusterKit.Log", "./ClusterKit.Log/ClusterKit.Log.Console/ClusterKit.Log.Console.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("ClusterKit.Log", "./ClusterKit.Log/ClusterKit.Log.ElasticSearch/ClusterKit.Log.ElasticSearch.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))


    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Descriptor/ClusterKit.Web.Descriptor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.CRUDS/ClusterKit.Web.CRUDS.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.Rest"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.SignalR/ClusterKit.Web.SignalR.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Swagger.Messages/ClusterKit.Web.Swagger.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage, ([||]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Swagger/ClusterKit.Web.Swagger.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Swagger.Monitor/ClusterKit.Web.Swagger.Monitor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Tests/ClusterKit.Web.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web.NginxConfigurator"; "ClusterKit.Web.SignalR"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web.Swagger.Messages"; "ClusterKit.Web.Swagger.Monitor"; "ClusterKit.Web.Swagger"; "ClusterKit.Web"|]))

    new ProjectDescription("ClusterKit.Monitoring", "./ClusterKit.Monitoring/ClusterKit.Monitoring/ClusterKit.Monitoring.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.SignalR"|]))
    //new ProjectDescription("./ClusterKit.Monitoring/ClusterKit.Monitoring.Tests/ClusterKit.Monitoring.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core";  "ClusterKit.Core.TestKit"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.SignalR"|]))

    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher.Messages/ClusterKit.NodeManager.Launcher.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.FallbackPackageDependencyFixer/ClusterKit.NodeManager.FallbackPackageDependencyFixer.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Client/ClusterKit.NodeManager.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.ConfigurationSource/ClusterKit.NodeManager.ConfigurationSource.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Core.EF"; "ClusterKit.Core.Data"|]))

    new ProjectDescription(
        "ClusterKit.NodeManager",
        "./ClusterKit.NodeManager/ClusterKit.NodeManager/ClusterKit.NodeManager.csproj",
        ProjectDescription.EnProjectType.NugetPackage,
        ([|
            "ClusterKit.Core";
            "ClusterKit.Core.EF";
            "ClusterKit.Core.Data";
            "ClusterKit.Core.Rest";
            "ClusterKit.Web.Client";
            "ClusterKit.Web.Descriptor";
            "ClusterKit.Web";
            "ClusterKit.Web.CRUDS";
            "ClusterKit.NodeManager.Client";
            "ClusterKit.NodeManager.ConfigurationSource";
            "ClusterKit.NodeManager.Launcher.Messages"
            |]))
    new ProjectDescription(
        "ClusterKit.NodeManager",
        "./ClusterKit.NodeManager/ClusterKit.NodeManager.Tests/ClusterKit.NodeManager.Tests.csproj",
        ProjectDescription.EnProjectType.XUnitTests,
        ([|
            "ClusterKit.Core";
            "ClusterKit.Core.TestKit";
            "ClusterKit.Core.Rest";
            "ClusterKit.Core.Data";
            "ClusterKit.Core.Data.TestKit";
            "ClusterKit.Core.EF";
            "ClusterKit.Core.EF.TestKit";
            "ClusterKit.Core.EF.Npgsql";
            "ClusterKit.Web.Client";
            "ClusterKit.Web.Descriptor";
            "ClusterKit.Web";
            "ClusterKit.Web.CRUDS";
            "ClusterKit.NodeManager";
            "ClusterKit.NodeManager.Client";
            "ClusterKit.NodeManager.ConfigurationSource";
            "ClusterKit.NodeManager.Launcher.Messages"
          |]))

|]

BuildUtils.DefineProjects projects