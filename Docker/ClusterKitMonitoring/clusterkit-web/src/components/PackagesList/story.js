import React from 'react';
import { storiesOf } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import PackagesList from './PackagesList';

storiesOf('Packages')
  .add('list', () => {
    const packagesList = getPackagesList();
    return <PackagesList packages={packagesList} />;
  })
;

let getPackagesList = function () {
  const packages = [{"BuildDate": null, "Id": "ClusterKit.Core", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Core.Service",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.LargeObjects", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.LargeObjects.Client",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Log.Console", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Log.ElasticSearch",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Monitoring.Client", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.NodeManager.Client",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.NodeManager.Launcher.Messages", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Security.Client",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Web.Client", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Web.NginxConfigurator",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Data", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Data.CRUD",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Data.EF", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Data.EF.Npgsql",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Monitoring", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.NodeManager",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.NodeManager.Authentication", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.NodeManager.ConfigurationSource",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Security.SessionRedis", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Web",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Web.Authentication", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Web.Authorization",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Web.Descriptor", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Web.Rest",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Web.Swagger", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Web.Swagger.Messages",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Web.Swagger.Monitor", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Build",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Core.TestKit", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Data.EF.TestKit",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.Data.TestKit", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.NodeManager.FallbackPackageDependencyFixer",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "ClusterKit.NodeManager.Launcher", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "ClusterKit.Web.SignalR",
    "Version": "0.0.0-local"
  }];
  return packages;
};
