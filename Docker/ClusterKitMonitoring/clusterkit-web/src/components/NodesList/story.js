import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import NodesList from './index';

storiesOf('Homepage')
  .add('nodesList without error, has privilege to upgrade', () => {
    const nodes = getNodesListRelay();
    const props = {
      nodeDescriptions: nodes.data.api.nodeManagerData,
      hasError: false,
      upgradeNodePrivilege: true,
      onManualUpgrade: action('onManualUpgrade')
    };
    return <StubContainer Component={NodesList} props={props} />;
  })
  .add('nodesList without error, no privilege to upgrade', () => {
    const nodes = getNodesListRelay();
    const props = {
      nodeDescriptions: nodes.data.api.nodeManagerData,
      hasError: false,
      upgradeNodePrivilege: false,
      onManualUpgrade: action('onManualUpgrade')
    };
    return <StubContainer Component={NodesList} props={props} />;
  })
  .add('nodesList with error', () => {
    const nodes = getNodesListRelay();
    const props = {
      nodeDescriptions: nodes.data.api.nodeManagerData,
      hasError: true,
      upgradeNodePrivilege: true,
      onManualUpgrade: action('onManualUpgrade')
    };
    return <StubContainer Component={NodesList} props={props} />;
  })
;

const getNodesListRelay = function () {
  return {
    "data": {
      "api": {
        "nodeManagerData": {
          "getActiveNodeDescriptions": [{
            "nodeTemplate": null,
            "containerType": null,
            "isClusterLeader": false,
            "isObsolete": false,
            "isInitialized": false,
            "leaderInRoles": [],
            "nodeId": "00000000-0000-0000-0000-000000000000",
            "nodeTemplateVersion": 0,
            "roles": [],
            "startTimeStamp": 0,
            "nodeAddress": {"host": "seed", "port": 3090},
            "modules": []
          }, {
            "nodeTemplate": "clusterManager",
            "containerType": "manager",
            "isClusterLeader": true,
            "isObsolete": false,
            "isInitialized": true,
            "leaderInRoles": ["NodeManager", "ClusterKit.API.Endpoint", "Web.Swagger.Monitor", "ClusterKit.Web.GraphQL.Publisher", "Monitoring", "Web.Swagger.Publish", "Web", "ClusterKit.Web.Authentication"],
            "nodeId": "56424d1d-a51a-4959-9cc9-eba0e3901c18",
            "nodeTemplateVersion": 0,
            "roles": ["ClusterKit.API.Endpoint", "Monitoring", "NodeManager", "ClusterKit.Web.Authentication", "ClusterKit.Web.GraphQL.Publisher", "Web.Swagger.Monitor", "Web.Swagger.Publish", "Web"],
            "startTimeStamp": 636249866934857710,
            "nodeAddress": {"host": "172.18.0.10", "port": 34024},
            "modules": [{"id": "ClusterKit.API.Endpoint", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Core",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Data.EF", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Data.EF.Npgsql",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.LargeObjects", "version": "0.0.0.0"}, {
              "id": "ClusterKit.LargeObjects.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Log.Console", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Log.ElasticSearch",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Monitoring", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Monitoring.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.NodeManager", "version": "0.0.0.0"}, {
              "id": "ClusterKit.NodeManager.Authentication",
              "version": "0.0.0.0"
            }, {
              "id": "ClusterKit.NodeManager.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.NodeManager.ConfigurationSource", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Security.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Security.SessionRedis", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Authentication", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web.Authorization",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Descriptor", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web.GraphQL.Publisher",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Rest", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web.Swagger",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Swagger.Monitor", "version": "0.0.0.0"}]
          }, {
            "nodeTemplate": "clusterManager",
            "containerType": "worker",
            "isClusterLeader": false,
            "isObsolete": false,
            "isInitialized": true,
            "leaderInRoles": [],
            "nodeId": "6fafc4c6-5d35-4c36-878d-dc30f1302a0b",
            "nodeTemplateVersion": 0,
            "roles": ["ClusterKit.API.Endpoint", "Monitoring", "NodeManager", "ClusterKit.Web.Authentication", "ClusterKit.Web.GraphQL.Publisher", "Web.Swagger.Monitor", "Web.Swagger.Publish", "Web"],
            "startTimeStamp": 636249867116419490,
            "nodeAddress": {"host": "172.18.0.11", "port": 38585},
            "modules": [{"id": "ClusterKit.API.Endpoint", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Core",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Data.EF", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Data.EF.Npgsql",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.LargeObjects", "version": "0.0.0.0"}, {
              "id": "ClusterKit.LargeObjects.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Log.Console", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Log.ElasticSearch",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Monitoring", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Monitoring.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.NodeManager", "version": "0.0.0.0"}, {
              "id": "ClusterKit.NodeManager.Authentication",
              "version": "0.0.0.0"
            }, {
              "id": "ClusterKit.NodeManager.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.NodeManager.ConfigurationSource", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Security.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Security.SessionRedis", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Authentication", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web.Authorization",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Descriptor", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web.GraphQL.Publisher",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Rest", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Web.Swagger",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.Swagger.Monitor", "version": "0.0.0.0"}]
          }, {
            "nodeTemplate": "publisher",
            "containerType": "publisher",
            "isClusterLeader": false,
            "isObsolete": false,
            "isInitialized": true,
            "leaderInRoles": ["Web.Nginx"],
            "nodeId": "080cd755-91cd-4bb1-8da4-ef1c0bd29033",
            "nodeTemplateVersion": 0,
            "roles": ["Web.Nginx"],
            "startTimeStamp": 636249866739159300,
            "nodeAddress": {"host": "publisher1", "port": 40341},
            "modules": [{"id": "ClusterKit.Core", "version": "0.0.0.0"}, {
              "id": "ClusterKit.LargeObjects",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.LargeObjects.Client", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Log.Console",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Log.ElasticSearch", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Monitoring.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.NodeManager.Client", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Security.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.NginxConfigurator", "version": "0.0.0.0"}]
          }, {
            "nodeTemplate": "publisher",
            "containerType": "publisher",
            "isClusterLeader": false,
            "isObsolete": false,
            "isInitialized": true,
            "leaderInRoles": [],
            "nodeId": "072d4b44-6162-4d88-ace2-37bab167699f",
            "nodeTemplateVersion": 0,
            "roles": ["Web.Nginx"],
            "startTimeStamp": 636249866806460040,
            "nodeAddress": {"host": "publisher2", "port": 45528},
            "modules": [{"id": "ClusterKit.Core", "version": "0.0.0.0"}, {
              "id": "ClusterKit.LargeObjects",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.LargeObjects.Client", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Log.Console",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Log.ElasticSearch", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Monitoring.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.NodeManager.Client", "version": "0.0.0.0"}, {
              "id": "ClusterKit.Security.Client",
              "version": "0.0.0.0"
            }, {"id": "ClusterKit.Web.NginxConfigurator", "version": "0.0.0.0"}]
          }]
        }
      }
    }
  };
};
