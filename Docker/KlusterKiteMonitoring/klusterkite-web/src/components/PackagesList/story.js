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
  const packages = [{"BuildDate": null, "Id": "KlusterKite.Core", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Core.Service",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.LargeObjects", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.LargeObjects.Client",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Log.Console", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Log.ElasticSearch",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Monitoring.Client", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.NodeManager.Client",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.NodeManager.Launcher.Messages", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Security.Client",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Web.Client", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Web.NginxConfigurator",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Data", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Data.CRUD",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Data.EF", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Data.EF.Npgsql",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Monitoring", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.NodeManager",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.NodeManager.Authentication", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.NodeManager.ConfigurationSource",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Security.SessionRedis", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Web",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Web.Authentication", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Web.Authorization",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Web.Descriptor", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Web.Rest",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Web.Swagger", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Web.Swagger.Messages",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Web.Swagger.Monitor", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Build",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Core.TestKit", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Data.EF.TestKit",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.Data.TestKit", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.NodeManager.FallbackPackageDependencyFixer",
    "Version": "0.0.0-local"
  }, {"BuildDate": null, "Id": "KlusterKite.NodeManager.Launcher", "Version": "0.0.0-local"}, {
    "BuildDate": null,
    "Id": "KlusterKite.Web.SignalR",
    "Version": "0.0.0-local"
  }];
  return packages;
};
