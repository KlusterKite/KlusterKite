import React from 'react';
import { storiesOf } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import MigratorTemplatesList from './MigratorTemplatesList';

storiesOf('Migrator Templates')
  .add('list, full access', () => {
    const nodes = getTemplatesListRelay();
    const props = {
      templates: nodes.data.api.clusterKitNodesApi,
      createMigratorTemplatePrivilege: true,
      getMigratorTemplatePrivilege: true,
    };
    return <StubContainer Component={MigratorTemplatesList} props={props} />;
  })
  .add('list, no access to create', () => {
    const nodes = getTemplatesListRelay();
    const props = {
      templates: nodes.data.api.clusterKitNodesApi,
      createMigratorTemplatePrivilege: false,
      getMigratorTemplatePrivilege: true,
    };
    return <StubContainer Component={MigratorTemplatesList} props={props} />;
  })
  .add('list, no access to edit', () => {
    const nodes = getTemplatesListRelay();
    const props = {
      templates: nodes.data.api.clusterKitNodesApi,
      createMigratorTemplatePrivilege: true,
      getMigratorTemplatePrivilege: false,
    };
    return <StubContainer Component={MigratorTemplatesList} props={props} />;
  })
  .add('list, no access to create/edit', () => {
    const nodes = getTemplatesListRelay();
    const props = {
      templates: nodes.data.api.clusterKitNodesApi,
      createMigratorTemplatePrivilege: false,
      getMigratorTemplatePrivilege: false,
    };
    return <StubContainer Component={MigratorTemplatesList} props={props} />;
  })
;

let getTemplatesList = function () {
  const templates = [
    {
      "Code": "clusterManager",
      "Id": 2,
      "MaximumNeededInstances": 3,
      "MinimumRequiredInstances": 1,
      "Name": "Cluster manager (cluster monitoring and managing)",
      "Packages": [
        "ClusterKit.Core.Service",
        "ClusterKit.NodeManager.Client",
        "ClusterKit.Monitoring.Client",
        "ClusterKit.Monitoring",
        "ClusterKit.NodeManager",
        "ClusterKit.Data.EF.Npgsql",
        "ClusterKit.Web.Swagger.Monitor",
        "ClusterKit.Web.Swagger",
        "ClusterKit.Log.Console",
        "ClusterKit.Log.ElasticSearch",
        "ClusterKit.Web.Authentication",
        "ClusterKit.Security.SessionRedis"
      ],
      "Priority": 100.0,
      "Version": 0
    },
    {
      "Code": "empty",
      "Id": 3,
      "MaximumNeededInstances": null,
      "MinimumRequiredInstances": 0,
      "Name": "Cluster empty instance, just for demo",
      "Packages": [
        "ClusterKit.Core.Service",
        "ClusterKit.NodeManager.Client",
        "ClusterKit.Monitoring.Client"
      ],
      "Priority": 1.0,
      "Version": 0
    },
    {
      "Code": "publisher",
      "Id": 1,
      "MaximumNeededInstances": null,
      "MinimumRequiredInstances": 1,
      "Name": "Cluster Nginx configurator",
      "Packages": [
        "ClusterKit.Core.Service",
        "ClusterKit.Web.NginxConfigurator",
        "ClusterKit.NodeManager.Client",
        "ClusterKit.Log.Console",
        "ClusterKit.Log.ElasticSearch",
        "ClusterKit.Monitoring.Client"
      ],
      "Priority": 1000.0,
      "Version": 0
    }
  ];
  return templates;
};

const getTemplatesListRelay = () => {
  return {
    "data": {
      "api": {
        "clusterKitNodesApi": {
          "nodeTemplates": {
            "edges": [{
              "node": {
                "id": "{\"p\":[{\"f\":\"nodeManagerData\",\"a\":{}},{\"f\":\"nodeTemplates\"}],\"api\":\"ClusterKitNodeApi\",\"id\":1}",
                "code": "publisher",
                "minimumRequiredInstances": 1,
                "maximumNeededInstances": null,
                "name": "Cluster Nginx configurator",
                "packages": ["ClusterKit.Core.Service", "ClusterKit.Web.NginxConfigurator", "ClusterKit.NodeManager.Client", "ClusterKit.Log.Console", "ClusterKit.Log.ElasticSearch", "ClusterKit.Monitoring.Client"],
                "priority": 1000.0,
                "version": 0
              }, "cursor": null
            }, {
              "node": {
                "id": "{\"p\":[{\"f\":\"nodeManagerData\",\"a\":{}},{\"f\":\"nodeTemplates\"}],\"api\":\"ClusterKitNodeApi\",\"id\":2}",
                "code": "clusterManager",
                "minimumRequiredInstances": 1,
                "maximumNeededInstances": 3,
                "name": "Cluster manager (cluster monitoring and managing)",
                "packages": ["ClusterKit.Core.Service", "ClusterKit.NodeManager.Client", "ClusterKit.Monitoring.Client", "ClusterKit.Monitoring", "ClusterKit.NodeManager", "ClusterKit.Data.EF.Npgsql", "ClusterKit.Web.Swagger.Monitor", "ClusterKit.Web.Swagger", "ClusterKit.Log.Console", "ClusterKit.Log.ElasticSearch", "ClusterKit.Web.Authentication", "ClusterKit.Security.SessionRedis", "ClusterKit.API.Endpoint", "ClusterKit.Web.GraphQL.Publisher"],
                "priority": 100.0,
                "version": 0
              }, "cursor": null
            }, {
              "node": {
                "id": "{\"p\":[{\"f\":\"nodeManagerData\",\"a\":{}},{\"f\":\"nodeTemplates\"}],\"api\":\"ClusterKitNodeApi\",\"id\":3}",
                "code": "empty",
                "minimumRequiredInstances": 0,
                "maximumNeededInstances": null,
                "name": "Cluster empty instance, just for demo",
                "packages": ["ClusterKit.Core.Service", "ClusterKit.NodeManager.Client", "ClusterKit.Monitoring.Client"],
                "priority": 1.0,
                "version": 0
              }, "cursor": null
            }]
          }
        }
      }
    }
  };
};
