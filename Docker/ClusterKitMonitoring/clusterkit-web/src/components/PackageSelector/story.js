import React from 'react';
import { storiesOf } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import PackagesSelector from './index';

storiesOf('Packages')
  .add('input with autocomplete', () => {
    const packagesList = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <PackagesSelector packages={packagesList} />;
  })
;

const getPackages = () => {
  return {
    "data": {
      "api": {
        "clusterKitNodesApi": {
          "nugetPackages": {
            "edges": [
              {
                "node": {
                  "name": "ClusterKit.API.Client",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local",
                    "0.0.1",
                    "0.0.12",
                    "0.0.2",
                    "0.0.5 beta",
                    "0.1.0",
                    "0.1.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Core",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Core.Service",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Data.CRUD",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.LargeObjects",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.LargeObjects.Client",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Log.Console",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Log.ElasticSearch",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Monitoring.Client",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.NodeManager.Client",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.NodeManager.Launcher.Messages",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Security.Client",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Client",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.API.Endpoint",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.API.Provider",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Data",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Data.EF",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Data.EF.Npgsql",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Monitoring",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.NodeManager",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.NodeManager.Authentication",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.NodeManager.ConfigurationSource",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Security.SessionRedis",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Authentication",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Authorization",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Descriptor",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.GraphQL.Publisher",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.NginxConfigurator",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Rest",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Swagger",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Swagger.Messages",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.Swagger.Monitor",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.Cluster",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.Cluster.Sharding",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.Cluster.Tools",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.DI.CastleWindsor",
                  "version": "1.0.8.191-beta",
                  "availableVersions": [
                    "1.0.8.191-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.DI.Core",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.DI.TestKit",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.Logger.Serilog",
                  "version": "1.1.3.12-beta",
                  "availableVersions": [
                    "1.1.3.12-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.Persistence",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.Remote",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.Serialization.Hyperion",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.TestKit",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "Akka.TestKit.Xunit2",
                  "version": "1.1.3.344-beta",
                  "availableVersions": [
                    "1.1.3.344-beta"
                  ]
                }
              },
              {
                "node": {
                  "name": "BCrypt-Official",
                  "version": "0.1.109",
                  "availableVersions": [
                    "0.1.109"
                  ]
                }
              },
              {
                "node": {
                  "name": "Castle.Core",
                  "version": "3.3.3",
                  "availableVersions": [
                    "3.3.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Castle.Windsor",
                  "version": "3.4.0",
                  "availableVersions": [
                    "3.4.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.API.Tests",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Build",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Core.TestKit",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Data.EF.TestKit",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Data.TestKit",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.NodeManager.FallbackPackageDependencyFixer",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.NodeManager.Launcher",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "ClusterKit.Web.SignalR",
                  "version": "0.0.0-local",
                  "availableVersions": [
                    "0.0.0-local"
                  ]
                }
              },
              {
                "node": {
                  "name": "CommonServiceLocator",
                  "version": "1.3",
                  "availableVersions": [
                    "1.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "CommonServiceLocator.WindsorAdapter",
                  "version": "1.0",
                  "availableVersions": [
                    "1.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "DotNetty.Buffers",
                  "version": "0.3.2",
                  "availableVersions": [
                    "0.3.2"
                  ]
                }
              },
              {
                "node": {
                  "name": "DotNetty.Codecs",
                  "version": "0.3.2",
                  "availableVersions": [
                    "0.3.2"
                  ]
                }
              },
              {
                "node": {
                  "name": "DotNetty.Common",
                  "version": "0.3.2",
                  "availableVersions": [
                    "0.3.2"
                  ]
                }
              },
              {
                "node": {
                  "name": "DotNetty.Handlers",
                  "version": "0.3.2",
                  "availableVersions": [
                    "0.3.2"
                  ]
                }
              },
              {
                "node": {
                  "name": "DotNetty.Transport",
                  "version": "0.3.2",
                  "availableVersions": [
                    "0.3.2"
                  ]
                }
              },
              {
                "node": {
                  "name": "Effort.EF6",
                  "version": "1.3.0",
                  "availableVersions": [
                    "1.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Elasticsearch.Net",
                  "version": "5.0.1",
                  "availableVersions": [
                    "5.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "EntityFramework",
                  "version": "6.1.3",
                  "availableVersions": [
                    "6.1.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "EntityFramework6.Npgsql",
                  "version": "3.1.1",
                  "availableVersions": [
                    "3.1.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "FAKE",
                  "version": "4.50.0",
                  "availableVersions": [
                    "4.50.0",
                    "4.46.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "FSharp.Core",
                  "version": "4.0.0.1",
                  "availableVersions": [
                    "4.0.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Google.ProtocolBuffers",
                  "version": "2.4.1.555",
                  "availableVersions": [
                    "2.4.1.555"
                  ]
                }
              },
              {
                "node": {
                  "name": "GraphQL",
                  "version": "0.14.6.657",
                  "availableVersions": [
                    "0.14.6.657"
                  ]
                }
              },
              {
                "node": {
                  "name": "GraphQL-Parser",
                  "version": "2.0.0",
                  "availableVersions": [
                    "2.0.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Helios",
                  "version": "2.1.3",
                  "availableVersions": [
                    "2.1.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Hyperion",
                  "version": "0.9.2",
                  "availableVersions": [
                    "0.9.2"
                  ]
                }
              },
              {
                "node": {
                  "name": "JetBrains.Annotations",
                  "version": "10.2.1",
                  "availableVersions": [
                    "10.2.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.Cors",
                  "version": "5.2.3",
                  "availableVersions": [
                    "5.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.Mvc",
                  "version": "5.2.3",
                  "availableVersions": [
                    "5.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.Razor",
                  "version": "3.2.3",
                  "availableVersions": [
                    "3.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.SignalR.Core",
                  "version": "2.2.1",
                  "availableVersions": [
                    "2.2.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.SignalR.Utils",
                  "version": "2.2.1",
                  "availableVersions": [
                    "2.2.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.WebApi.Client",
                  "version": "5.2.3",
                  "availableVersions": [
                    "5.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.WebApi.Core",
                  "version": "5.2.3",
                  "availableVersions": [
                    "5.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.WebApi.Cors",
                  "version": "5.2.3",
                  "availableVersions": [
                    "5.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.WebApi.Owin",
                  "version": "5.2.3",
                  "availableVersions": [
                    "5.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.WebApi.OwinSelfHost",
                  "version": "5.2.3",
                  "availableVersions": [
                    "5.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.AspNet.WebPages",
                  "version": "3.2.3",
                  "availableVersions": [
                    "3.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Bcl.Immutable",
                  "version": "1.0.34",
                  "availableVersions": [
                    "1.0.34"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.NETCore.Platforms",
                  "version": "1.1.0",
                  "availableVersions": [
                    "1.1.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Owin",
                  "version": "3.0.1",
                  "availableVersions": [
                    "3.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Owin.Cors",
                  "version": "3.0.1",
                  "availableVersions": [
                    "3.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Owin.Host.HttpListener",
                  "version": "3.0.1",
                  "availableVersions": [
                    "3.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Owin.Hosting",
                  "version": "3.0.1",
                  "availableVersions": [
                    "3.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Owin.Security",
                  "version": "3.0.1",
                  "availableVersions": [
                    "3.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Owin.Security.OAuth",
                  "version": "3.0.1",
                  "availableVersions": [
                    "3.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Web.Infrastructure",
                  "version": "1.0.0.0",
                  "availableVersions": [
                    "1.0.0.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Microsoft.Web.Xdt",
                  "version": "2.1.1",
                  "availableVersions": [
                    "2.1.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "NMemory",
                  "version": "1.1.0",
                  "availableVersions": [
                    "1.1.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Newtonsoft.Json",
                  "version": "9.0.1",
                  "availableVersions": [
                    "9.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Npgsql",
                  "version": "3.1.9",
                  "availableVersions": [
                    "3.1.9"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Client",
                  "version": "3.5.0",
                  "availableVersions": [
                    "3.5.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Common",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Configuration",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.ContentModel",
                  "version": "3.5.0",
                  "availableVersions": [
                    "3.5.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Core",
                  "version": "2.14.0",
                  "availableVersions": [
                    "2.14.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Frameworks",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Packaging",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Packaging.Core",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Packaging.Core.Types",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Protocol.Core.Types",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Protocol.Core.v3",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Repositories",
                  "version": "3.5.0",
                  "availableVersions": [
                    "3.5.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.RuntimeModel",
                  "version": "3.5.0",
                  "availableVersions": [
                    "3.5.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "NuGet.Versioning",
                  "version": "4.0.0-rc2",
                  "availableVersions": [
                    "4.0.0-rc2"
                  ]
                }
              },
              {
                "node": {
                  "name": "Owin",
                  "version": "1.0",
                  "availableVersions": [
                    "1.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "RestSharp",
                  "version": "105.2.3",
                  "availableVersions": [
                    "105.2.3"
                  ]
                }
              },
              {
                "node": {
                  "name": "Serilog",
                  "version": "2.4.0",
                  "availableVersions": [
                    "2.4.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Serilog.Sinks.ColoredConsole",
                  "version": "2.0.0",
                  "availableVersions": [
                    "2.0.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Serilog.Sinks.Elasticsearch",
                  "version": "4.1.1",
                  "availableVersions": [
                    "4.1.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "Serilog.Sinks.File",
                  "version": "3.2.0",
                  "availableVersions": [
                    "3.2.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Serilog.Sinks.PeriodicBatching",
                  "version": "2.0.0",
                  "availableVersions": [
                    "2.0.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Serilog.Sinks.RollingFile",
                  "version": "3.3.0",
                  "availableVersions": [
                    "3.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Serilog.Sinks.TextWriter",
                  "version": "2.1.0",
                  "availableVersions": [
                    "2.1.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "StackExchange.Redis.Mono",
                  "version": "1.0.0",
                  "availableVersions": [
                    "1.0.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "Swashbuckle.Core.Net45",
                  "version": "5.2.1",
                  "availableVersions": [
                    "5.2.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Collections",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Collections.Concurrent",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Collections.Immutable",
                  "version": "1.3.1",
                  "availableVersions": [
                    "1.3.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.ComponentModel",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Diagnostics.Debug",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Diagnostics.DiagnosticSource",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Diagnostics.Tools",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Diagnostics.Tracing",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Globalization",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.IO",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.IO.Compression",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Linq",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Linq.Expressions",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Net.Primitives",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.ObjectModel",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Reflection",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Reflection.Extensions",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Reflection.Primitives",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Resources.ResourceManager",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Runtime",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Runtime.Extensions",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Runtime.Handles",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Runtime.InteropServices",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Runtime.InteropServices.RuntimeInformation",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Runtime.Numerics",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Text.Encoding",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Text.Encoding.Extensions",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Text.RegularExpressions",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Threading",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Threading.Tasks",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Threading.Timer",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "System.Xml.XDocument",
                  "version": "4.3.0",
                  "availableVersions": [
                    "4.3.0"
                  ]
                }
              },
              {
                "node": {
                  "name": "docopt.net",
                  "version": "0.6.1.9",
                  "availableVersions": [
                    "0.6.1.9"
                  ]
                }
              },
              {
                "node": {
                  "name": "xunit",
                  "version": "2.2.0-beta5-build3474",
                  "availableVersions": [
                    "2.2.0-beta5-build3474"
                  ]
                }
              },
              {
                "node": {
                  "name": "xunit.abstractions",
                  "version": "2.0.1",
                  "availableVersions": [
                    "2.0.1"
                  ]
                }
              },
              {
                "node": {
                  "name": "xunit.assert",
                  "version": "2.2.0-beta5-build3474",
                  "availableVersions": [
                    "2.2.0-beta5-build3474"
                  ]
                }
              },
              {
                "node": {
                  "name": "xunit.core",
                  "version": "2.2.0-beta5-build3474",
                  "availableVersions": [
                    "2.2.0-beta5-build3474"
                  ]
                }
              },
              {
                "node": {
                  "name": "xunit.extensibility.core",
                  "version": "2.2.0-beta5-build3474",
                  "availableVersions": [
                    "2.2.0-beta5-build3474"
                  ]
                }
              },
              {
                "node": {
                  "name": "xunit.extensibility.execution",
                  "version": "2.2.0-beta5-build3474",
                  "availableVersions": [
                    "2.2.0-beta5-build3474"
                  ]
                }
              },
              {
                "node": {
                  "name": "xunit.runner.console",
                  "version": "2.2.0-beta5-build3474",
                  "availableVersions": [
                    "2.2.0-beta5-build3474"
                  ]
                }
              }
            ]
          }
        }
      }
    }
  }
};
