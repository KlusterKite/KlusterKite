#r @"BuildScripts/ClusterKit.Build.dll" // include budle of build utils

open  ClusterKit.Build

BuildUtils.DefineSolutionName "ClusterKit"

let projects = [|
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Build/ClusterKit.Build.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core/ClusterKit.Core.csproj", ProjectDescription.EnProjectType.NugetPackage)
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.TestKit/ClusterKit.Core.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.Service/ClusterKit.Core.Service.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))

    new ProjectDescription("ClusterKit.Core", "./ClusterKit.Core/ClusterKit.Core.Tests/ClusterKit.Core.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"|]))
        
    new ProjectDescription("ClusterKit.Log", "./ClusterKit.Log/ClusterKit.Log.Console/ClusterKit.Log.Console.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("ClusterKit.Log", "./ClusterKit.Log/ClusterKit.Log.ElasticSearch/ClusterKit.Log.ElasticSearch.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))

    new ProjectDescription("ClusterKit.Security", "./ClusterKit.Security/ClusterKit.Security.Attributes/ClusterKit.Security.Attributes.csproj", ProjectDescription.EnProjectType.NugetPackage, ([||]))
    new ProjectDescription("ClusterKit.Security", "./ClusterKit.Security/ClusterKit.Security.Client/ClusterKit.Security.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Attributes"|]))
    new ProjectDescription("ClusterKit.Security", "./ClusterKit.Security/ClusterKit.Security.SessionRedis/ClusterKit.Security.SessionRedis.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Security", "./ClusterKit.Security/ClusterKit.Security.Tests/ClusterKit.Security.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.Security.SessionRedis"|]))

    new ProjectDescription("ClusterKit.LargeObjects", "./ClusterKit.LargeObjects/ClusterKit.LargeObjects.Client/ClusterKit.LargeObjects.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"|]))
    new ProjectDescription("ClusterKit.LargeObjects", "./ClusterKit.LargeObjects/ClusterKit.LargeObjects/ClusterKit.LargeObjects.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.LargeObjects.Client"|]))   
    new ProjectDescription("ClusterKit.LargeObjects", "./ClusterKit.LargeObjects/ClusterKit.LargeObjects.Tests/ClusterKit.LargeObjects.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"|]))

    new ProjectDescription("ClusterKit.API", "./ClusterKit.API/ClusterKit.API.Attributes/ClusterKit.API.Attributes.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.API", "./ClusterKit.API/ClusterKit.API.Client/ClusterKit.API.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.API.Attributes"|]))
    new ProjectDescription("ClusterKit.API", "./ClusterKit.API/ClusterKit.API.Provider/ClusterKit.API.Provider.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.API.Client"; "ClusterKit.API.Attributes";|]))
    new ProjectDescription("ClusterKit.API", "./ClusterKit.API/ClusterKit.API.Endpoint/ClusterKit.API.Endpoint.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.API.Provider"|]))
    new ProjectDescription("ClusterKit.API", "./ClusterKit.API/ClusterKit.API.Tests/ClusterKit.API.Tests.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.API.Provider"; "ClusterKit.API.Endpoint"; "ClusterKit.Core.TestKit"|]))

    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data.CRUD/ClusterKit.Data.CRUD.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.LargeObjects.Client"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data/ClusterKit.Data.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Data.CRUD"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data.TestKit/ClusterKit.Data.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.CRUD"; "ClusterKit.Data"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))       
    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data.EF/ClusterKit.Data.EF.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.CRUD"; "ClusterKit.Data"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data.EF.TestKit/ClusterKit.Data.EF.TestKit.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.CRUD"; "ClusterKit.Data"; "ClusterKit.Data.EF"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data.EF.Npgsql/ClusterKit.Data.EF.Npgsql.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.EF"; "ClusterKit.Data"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data.EF.Effort/ClusterKit.Data.EF.Effort.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.EF"; "ClusterKit.Data"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Data", "./ClusterKit.Data/ClusterKit.Data.Tests/ClusterKit.Data.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Core.TestKit"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Data.CRUD"; "ClusterKit.Data"; "ClusterKit.Data.TestKit"; "ClusterKit.Data.EF"; "ClusterKit.Data.EF.TestKit"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  |]))


    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Client/ClusterKit.Web.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Descriptor/ClusterKit.Web.Descriptor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web/ClusterKit.Web.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Authorization/ClusterKit.Web.Authorization.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.Security.Client"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Authentication/ClusterKit.Web.Authentication.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.Web.Authorization"|]))    
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Rest/ClusterKit.Web.Rest.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Data.CRUD"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.Web.Authorization";|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.NginxConfigurator/ClusterKit.Web.NginxConfigurator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.SignalR/ClusterKit.Web.SignalR.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"|]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Swagger.Messages/ClusterKit.Web.Swagger.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Swagger/ClusterKit.Web.Swagger.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; |]))
    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.Swagger.Monitor/ClusterKit.Web.Swagger.Monitor.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Web.Swagger.Messages"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes"; "ClusterKit.Web.Authorization"|]))

    new ProjectDescription("ClusterKit.Web", "./ClusterKit.Web/ClusterKit.Web.GraphQL.Publisher/ClusterKit.Web.GraphQL.Publisher.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.Web.Authorization"; "ClusterKit.API.Client"|]))
    
    new ProjectDescription(
        "ClusterKit.Web", 
        "./ClusterKit.Web/ClusterKit.Web.Tests/ClusterKit.Web.Tests.csproj",
         ProjectDescription.EnProjectType.XUnitTests, 
         ([|
             "ClusterKit.Core"; 
             "ClusterKit.Core.TestKit"; 
             "ClusterKit.Web.Client"; 
             "ClusterKit.Web.NginxConfigurator"; 
             "ClusterKit.Web.SignalR"; 
             "ClusterKit.Web.Descriptor"; 
             "ClusterKit.Web.Swagger.Messages"; 
             "ClusterKit.Web.Swagger.Monitor"; 
             "ClusterKit.Web.Swagger"; 
             "ClusterKit.Web"; 
             "ClusterKit.Web.Authentication"; 
             "ClusterKit.Security.Client"; 
             "ClusterKit.Security.Attributes";  
             "ClusterKit.Web.Authorization";
             "ClusterKit.Web.GraphQL.Publisher";
             "ClusterKit.API.Client"; "ClusterKit.API.Attributes";
             "ClusterKit.API.Provider";
             "ClusterKit.API.Tests";
         |]))

    new ProjectDescription("ClusterKit.Monitoring", "./ClusterKit.Monitoring/ClusterKit.Monitoring.Client/ClusterKit.Monitoring.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.API.Client"; "ClusterKit.API.Attributes";|]))
    new ProjectDescription("ClusterKit.Monitoring", "./ClusterKit.Monitoring/ClusterKit.Monitoring/ClusterKit.Monitoring.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Monitoring.Client"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.Web.Authorization"|]))
    new ProjectDescription("ClusterKit.Monitoring", "./ClusterKit.Monitoring/ClusterKit.Monitoring.Tests/ClusterKit.Monitoring.Tests.csproj", ProjectDescription.EnProjectType.XUnitTests, ([|"ClusterKit.Core"; "ClusterKit.Web.Client"; "ClusterKit.Web"; "ClusterKit.Monitoring.Client"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.Web.Authorization"; "ClusterKit.Monitoring"; "ClusterKit.Web.Descriptor"; "ClusterKit.Web.Authorization"; "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.Api.Provider"; "ClusterKit.Web.GraphQL.Publisher";|]))
    

    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher.Messages/ClusterKit.NodeManager.Launcher.Messages.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Security.Attributes"; "ClusterKit.API.Attributes";|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Launcher/ClusterKit.NodeManager.Launcher.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Security.Attributes"; "ClusterKit.API.Attributes";"ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.FallbackPackageDependencyFixer/ClusterKit.NodeManager.FallbackPackageDependencyFixer.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Security.Attributes"; "ClusterKit.API.Attributes"; "ClusterKit.NodeManager.Launcher.Messages"|]))

    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Migrator/ClusterKit.NodeManager.Migrator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Security.Attributes";  "ClusterKit.API.Attributes"; |]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Migrator.EF/ClusterKit.NodeManager.Migrator.EF.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Security.Attributes";  "ClusterKit.API.Attributes"; "ClusterKit.NodeManager.Migrator";|]))
    
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Seeder.Launcher/ClusterKit.NodeManager.Seeder.Launcher.csproj", ProjectDescription.EnProjectType.NugetPackage, ([||]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Seeder/ClusterKit.NodeManager.Seeder.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Security.Attributes";  "ClusterKit.API.Attributes"; "ClusterKit.NodeManager.Migrator";|]))

    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Client/ClusterKit.NodeManager.Client.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.NodeManager.Launcher.Messages"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.Data.CRUD"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.Authentication/ClusterKit.NodeManager.Authentication.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.NodeManager.Launcher.Messages"; "ClusterKit.NodeManager.Client"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.Data.CRUD"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.ConfigurationSource/ClusterKit.NodeManager.ConfigurationSource.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.EF"; "ClusterKit.Data"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Data.CRUD"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.NodeManager.Client"; "ClusterKit.Web.Swagger.Messages"; "ClusterKit.Monitoring.Client"; "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.NodeManager.Launcher.Messages"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.ConfigurationSource.Migrator/ClusterKit.NodeManager.ConfigurationSource.Migrator.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.EF"; "ClusterKit.Data"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Data.CRUD"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.NodeManager.Client"; "ClusterKit.Web.Swagger.Messages"; "ClusterKit.Monitoring.Client"; "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.NodeManager.Launcher.Messages"; "ClusterKit.NodeManager.Migrator"; "ClusterKit.NodeManager.Migrator.EF"; "ClusterKit.NodeManager.ConfigurationSource"|]))
    new ProjectDescription("ClusterKit.NodeManager", "./ClusterKit.NodeManager/ClusterKit.NodeManager.ConfigurationSource.Seeder/ClusterKit.NodeManager.ConfigurationSource.Seeder.csproj", ProjectDescription.EnProjectType.NugetPackage, ([|"ClusterKit.Core"; "ClusterKit.Data.EF"; "ClusterKit.Data"; "ClusterKit.LargeObjects.Client"; "ClusterKit.LargeObjects"; "ClusterKit.Data.CRUD"; "ClusterKit.Security.Client"; "ClusterKit.Security.Attributes";  "ClusterKit.NodeManager.Client"; "ClusterKit.Web.Swagger.Messages"; "ClusterKit.Monitoring.Client"; "ClusterKit.API.Client"; "ClusterKit.API.Attributes"; "ClusterKit.NodeManager.Launcher.Messages"; "ClusterKit.NodeManager.Migrator"; "ClusterKit.NodeManager.Migrator.EF"; "ClusterKit.NodeManager.ConfigurationSource"; "ClusterKit.NodeManager.ConfigurationSource.Migrator"|]))

    new ProjectDescription(
        "ClusterKit.NodeManager",
        "./ClusterKit.NodeManager/ClusterKit.NodeManager/ClusterKit.NodeManager.csproj",
        ProjectDescription.EnProjectType.NugetPackage,
        ([|
            "ClusterKit.Core"; 
            "ClusterKit.LargeObjects.Client"; 
            "ClusterKit.LargeObjects";
            "ClusterKit.Data.EF";
            "ClusterKit.Data";
            "ClusterKit.Data.CRUD";
            "ClusterKit.Web.Client";
            "ClusterKit.Web.Descriptor";
            "ClusterKit.Web";
            "ClusterKit.Web.Rest";
            "ClusterKit.NodeManager.Client";
            "ClusterKit.NodeManager.ConfigurationSource";
            "ClusterKit.NodeManager.Launcher.Messages";
            "ClusterKit.NodeManager.Authentication";
            "ClusterKit.Security.Client"; 
            "ClusterKit.Security.Attributes"; 
            "ClusterKit.Web.Authorization";           
            "ClusterKit.Web.Swagger.Messages"; 
            "ClusterKit.Monitoring.Client";
            "ClusterKit.API.Client"; 
            "ClusterKit.API.Attributes"; 
            "ClusterKit.API.Provider";
            "ClusterKit.NodeManager.Migrator";
            |]))
    new ProjectDescription(
        "ClusterKit.NodeManager",
        "./ClusterKit.NodeManager/ClusterKit.NodeManager.Tests/ClusterKit.NodeManager.Tests.csproj",
        ProjectDescription.EnProjectType.XUnitTests,
        ([|
            "ClusterKit.Core";
            "ClusterKit.Core.TestKit"; 
            "ClusterKit.LargeObjects.Client"; 
            "ClusterKit.LargeObjects";
            "ClusterKit.Data.CRUD";
            "ClusterKit.Data";
            "ClusterKit.Data.TestKit";
            "ClusterKit.Data.EF";
            "ClusterKit.Data.EF.TestKit";
            "ClusterKit.Data.EF.Effort";
            "ClusterKit.Web.Client";
            "ClusterKit.Web.Descriptor";
            "ClusterKit.Web";
            "ClusterKit.Web.Rest";
            "ClusterKit.NodeManager";
            "ClusterKit.NodeManager.Client";
            "ClusterKit.NodeManager.ConfigurationSource";
            "ClusterKit.NodeManager.ConfigurationSource.Migrator";
            "ClusterKit.NodeManager.Launcher.Messages";
            "ClusterKit.NodeManager.Authentication";
            "ClusterKit.Security.Client"; 
            "ClusterKit.Security.Attributes"; 
            "ClusterKit.Web.Authorization";
            "ClusterKit.Web.Swagger.Messages"; 
            "ClusterKit.Web.GraphQL.Publisher";
            "ClusterKit.Monitoring.Client";
            "ClusterKit.API.Client"; 
            "ClusterKit.API.Attributes"; 
            "ClusterKit.API.Provider";
            "ClusterKit.NodeManager.Migrator";
            "ClusterKit.NodeManager.Migrator.EF";
          |]))

|]

BuildUtils.DefineProjects projects