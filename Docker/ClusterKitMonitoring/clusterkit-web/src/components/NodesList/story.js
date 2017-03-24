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
        "__typename": "ClusterKitMonitoring_ClusterKitNodeApi",
        "nodeManagerData": {
          "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9XQ==",
          "getActiveNodeDescriptions": {
            "edges": [{
              "node": {
                "containerType": null,
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": false,
                "leaderInRoles": [],
                "nodeId": "f6e8b5a4-0a42-48a9-aaec-647d67899a7a",
                "nodeTemplate": null,
                "nodeTemplateVersion": 0,
                "roles": [],
                "startTimeStamp": 0,
                "nodeAddress": {
                  "host": "seed",
                  "port": 3090,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImY2ZThiNWE0LTBhNDItNDhhOS1hYWVjLTY0N2Q2Nzg5OWE3YSJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {"edges": []},
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImY2ZThiNWE0LTBhNDItNDhhOS1hYWVjLTY0N2Q2Nzg5OWE3YSJ9XQ=="
              }, "cursor": "f6e8b5a4-0a42-48a9-aaec-647d67899a7a"
            }, {
              "node": {
                "containerType": "manager",
                "isClusterLeader": true,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": ["Web", "ClusterKit.Web.Authentication", "NodeManager", "ClusterKit.API.Endpoint", "Web.Swagger.Monitor", "ClusterKit.Web.GraphQL.Publisher", "Monitoring", "Web.Swagger.Publish"],
                "nodeId": "a2cff665-61f6-452d-8fd0-8ee80bc40316",
                "nodeTemplate": "clusterManager",
                "nodeTemplateVersion": 0,
                "roles": ["ClusterKit.API.Endpoint", "Monitoring", "NodeManager", "ClusterKit.Web.Authentication", "ClusterKit.Web.GraphQL.Publisher", "Web.Swagger.Monitor", "Web.Swagger.Publish", "Web"],
                "startTimeStamp": 636259390245141410,
                "nodeAddress": {
                  "host": "172.18.0.10",
                  "port": 35006,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQVBJLkVuZHBvaW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.API.Endpoint 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Data 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRiAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Data.EF 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRi5OcGdzcWwgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Data.EF.Npgsql 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZyAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Monitoring 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQXV0aGVudGljYXRpb24gMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ29uZmlndXJhdGlvblNvdXJjZSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.ConfigurationSource 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuU2Vzc2lvblJlZGlzIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Security.SessionRedis 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhlbnRpY2F0aW9uIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhvcml6YXRpb24gMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Authorization 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkRlc2NyaXB0b3IgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Descriptor 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkdyYXBoUUwuUHVibGlzaGVyIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.GraphQL.Publisher 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlJlc3QgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Rest 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Swagger 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIuTW9uaXRvciAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Swagger.Monitor 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImEyY2ZmNjY1LTYxZjYtNDUyZC04ZmQwLThlZTgwYmM0MDMxNiJ9XQ=="
              }, "cursor": "a2cff665-61f6-452d-8fd0-8ee80bc40316"
            }, {
              "node": {
                "containerType": "worker",
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": [],
                "nodeId": "10075869-6709-4151-8e7e-fbda25feb192",
                "nodeTemplate": "clusterManager",
                "nodeTemplateVersion": 0,
                "roles": ["ClusterKit.API.Endpoint", "Monitoring", "NodeManager", "ClusterKit.Web.Authentication", "ClusterKit.Web.GraphQL.Publisher", "Web.Swagger.Monitor", "Web.Swagger.Publish", "Web"],
                "startTimeStamp": 636259390435577390,
                "nodeAddress": {
                  "host": "172.18.0.11",
                  "port": 42081,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQVBJLkVuZHBvaW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.API.Endpoint 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Data 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRiAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Data.EF 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRi5OcGdzcWwgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Data.EF.Npgsql 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZyAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Monitoring 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQXV0aGVudGljYXRpb24gMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ29uZmlndXJhdGlvblNvdXJjZSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.ConfigurationSource 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuU2Vzc2lvblJlZGlzIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Security.SessionRedis 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhlbnRpY2F0aW9uIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhvcml6YXRpb24gMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Authorization 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkRlc2NyaXB0b3IgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Descriptor 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkdyYXBoUUwuUHVibGlzaGVyIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.GraphQL.Publisher 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlJlc3QgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Rest 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Swagger 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIuTW9uaXRvciAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.Swagger.Monitor 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjEwMDc1ODY5LTY3MDktNDE1MS04ZTdlLWZiZGEyNWZlYjE5MiJ9XQ=="
              }, "cursor": "10075869-6709-4151-8e7e-fbda25feb192"
            }, {
              "node": {
                "containerType": "publisher",
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": ["Web.Nginx"],
                "nodeId": "b40cbf1c-6934-43c1-9373-e79ae7d7aa37",
                "nodeTemplate": "publisher",
                "nodeTemplateVersion": 0,
                "roles": ["Web.Nginx"],
                "startTimeStamp": 636259390111401640,
                "nodeAddress": {
                  "host": "publisher1",
                  "port": 44300,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLk5naW54Q29uZmlndXJhdG9yIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.NginxConfigurator 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI0MGNiZjFjLTY5MzQtNDNjMS05MzczLWU3OWFlN2Q3YWEzNyJ9XQ=="
              }, "cursor": "b40cbf1c-6934-43c1-9373-e79ae7d7aa37"
            }, {
              "node": {
                "containerType": "publisher",
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": [],
                "nodeId": "abaa5c57-de16-421d-9643-a9d93eacbc05",
                "nodeTemplate": "publisher",
                "nodeTemplateVersion": 0,
                "roles": ["Web.Nginx"],
                "startTimeStamp": 636259390188379340,
                "nodeAddress": {
                  "host": "publisher2",
                  "port": 36624,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLk5naW54Q29uZmlndXJhdG9yIDAuMC4wLjAifV0=",
                      "version": "0.0.0.0"
                    }, "cursor": "ClusterKit.Web.NginxConfigurator 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImFiYWE1YzU3LWRlMTYtNDIxZC05NjQzLWE5ZDkzZWFjYmMwNSJ9XQ=="
              }, "cursor": "abaa5c57-de16-421d-9643-a9d93eacbc05"
            }]
          }
        },
        "id": "W10="
      }
    }
  };
};
