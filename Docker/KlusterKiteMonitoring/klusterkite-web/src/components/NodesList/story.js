import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import NodesList from './NodesList';

storiesOf('Homepage')
  .add('nodesList without error, has privilege to upgrade', () => {
    const nodes = getNodesListRelay();
    const props = {
      nodeDescriptions: nodes.data.api.klusterKiteNodesApi,
      hasError: false,
      upgradeNodePrivilege: true,
      testMode: true
    };
    return <StubContainer Component={NodesList} props={props} />;
  })
  .add('nodesList without error, no privilege to upgrade', () => {
    const nodes = getNodesListRelay();
    const props = {
      nodeDescriptions: nodes.data.api.klusterKiteNodesApi,
      hasError: false,
      upgradeNodePrivilege: false,
      testMode: true
    };
    return <StubContainer Component={NodesList} props={props} />;
  })
  .add('nodesList with error', () => {
    const nodes = getNodesListRelay();
    const props = {
      nodeDescriptions: nodes.data.api.klusterKiteNodesApi,
      hasError: true,
      upgradeNodePrivilege: true,
      testMode: true
    };
    return <StubContainer Component={NodesList} props={props} />;
  })
;

const getNodesListRelay = function () {
  return {
    "data": {
      "api": {
        "__typename": "KlusterKiteMonitoring_KlusterKiteNodeApi",
        "klusterKiteNodesApi": {
          "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9XQ==",
          "getActiveNodeDescriptions": {
            "edges": [{
              "node": {
                "containerType": null,
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": false,
                "leaderInRoles": [],
                "nodeId": "e2b8d770-64c5-4a2e-8347-e9c35a0b69ed",
                "nodeTemplate": null,
                "nodeTemplateVersion": 0,
                "roles": [],
                "startTimeStamp": 0,
                "nodeAddress": {
                  "host": "seed",
                  "port": 3090,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImUyYjhkNzcwLTY0YzUtNGEyZS04MzQ3LWU5YzM1YTBiNjllZCJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {"edges": []},
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImUyYjhkNzcwLTY0YzUtNGEyZS04MzQ3LWU5YzM1YTBiNjllZCJ9XQ=="
              }, "cursor": "e2b8d770-64c5-4a2e-8347-e9c35a0b69ed"
            }, {
              "node": {
                "containerType": "manager",
                "isClusterLeader": true,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": ["NodeManager", "KlusterKite.API.Endpoint", "Web.Swagger.Monitor", "KlusterKite.Web.GraphQL.Publisher", "Monitoring", "Web.Swagger.Publish", "Web", "KlusterKite.Web.Authentication"],
                "nodeId": "18d3a40c-d636-433e-815e-f33908d444bf",
                "nodeTemplate": "clusterManager",
                "nodeTemplateVersion": 0,
                "roles": ["KlusterKite.API.Endpoint", "Monitoring", "NodeManager", "KlusterKite.Web.Authentication", "KlusterKite.Web.GraphQL.Publisher", "Web.Swagger.Monitor", "Web.Swagger.Publish", "Web"],
                "startTimeStamp": 636261946507820630,
                "nodeAddress": {
                  "host": "172.18.0.10",
                  "port": 44988,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQVBJLkVuZHBvaW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.API.Endpoint",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.API.Endpoint 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Core",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Data",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Data 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRiAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Data.EF",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Data.EF 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRi5OcGdzcWwgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Data.EF.Npgsql",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Data.EF.Npgsql 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.LargeObjects",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "name": "KlusterKite.LargeObjects.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.Console",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.ElasticSearch",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZyAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Monitoring",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Monitoring 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Monitoring.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.NodeManager",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQXV0aGVudGljYXRpb24gMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.NodeManager.Authentication",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.NodeManager.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ29uZmlndXJhdGlvblNvdXJjZSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.NodeManager.ConfigurationSource",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.ConfigurationSource 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Security.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuU2Vzc2lvblJlZGlzIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Security.SessionRedis",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Security.SessionRedis 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhlbnRpY2F0aW9uIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web.Authentication",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhvcml6YXRpb24gMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Authorization",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Authorization 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkRlc2NyaXB0b3IgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Descriptor",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Descriptor 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkdyYXBoUUwuUHVibGlzaGVyIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web.GraphQL.Publisher",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.GraphQL.Publisher 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlJlc3QgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Rest",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Rest 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Swagger",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Swagger 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIuTW9uaXRvciAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Web.Swagger.Monitor",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Swagger.Monitor 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjE4ZDNhNDBjLWQ2MzYtNDMzZS04MTVlLWYzMzkwOGQ0NDRiZiJ9XQ=="
              }, "cursor": "18d3a40c-d636-433e-815e-f33908d444bf"
            }, {
              "node": {
                "containerType": "worker",
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": [],
                "nodeId": "9185835a-244f-4b76-83a3-a457d01a9167",
                "nodeTemplate": "clusterManager",
                "nodeTemplateVersion": 0,
                "roles": ["KlusterKite.API.Endpoint", "Monitoring", "NodeManager", "KlusterKite.Web.Authentication", "KlusterKite.Web.GraphQL.Publisher", "Web.Swagger.Monitor", "Web.Swagger.Publish", "Web"],
                "startTimeStamp": 636261946721977580,
                "nodeAddress": {
                  "host": "172.18.0.11",
                  "port": 33446,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQVBJLkVuZHBvaW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.API.Endpoint",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.API.Endpoint 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Core",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Data",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Data 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRiAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Data.EF",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Data.EF 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRi5OcGdzcWwgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Data.EF.Npgsql",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Data.EF.Npgsql 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.LargeObjects",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "name": "KlusterKite.LargeObjects.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.Console",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.ElasticSearch",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZyAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Monitoring",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Monitoring 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Monitoring.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.NodeManager",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQXV0aGVudGljYXRpb24gMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.NodeManager.Authentication",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.NodeManager.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ29uZmlndXJhdGlvblNvdXJjZSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.NodeManager.ConfigurationSource",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.ConfigurationSource 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Security.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuU2Vzc2lvblJlZGlzIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Security.SessionRedis",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Security.SessionRedis 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhlbnRpY2F0aW9uIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web.Authentication",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Authentication 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhvcml6YXRpb24gMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Authorization",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Authorization 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkRlc2NyaXB0b3IgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Descriptor",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Descriptor 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkdyYXBoUUwuUHVibGlzaGVyIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web.GraphQL.Publisher",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.GraphQL.Publisher 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlJlc3QgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Rest",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Rest 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Web.Swagger",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Swagger 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIuTW9uaXRvciAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Web.Swagger.Monitor",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.Swagger.Monitor 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxODU4MzVhLTI0NGYtNGI3Ni04M2EzLWE0NTdkMDFhOTE2NyJ9XQ=="
              }, "cursor": "9185835a-244f-4b76-83a3-a457d01a9167"
            }, {
              "node": {
                "containerType": "publisher",
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": ["Web.Nginx"],
                "nodeId": "b5e19426-be66-4e0d-abbd-567d319690ca",
                "nodeTemplate": "publisher",
                "nodeTemplateVersion": 0,
                "roles": ["Web.Nginx"],
                "startTimeStamp": 636261946400138170,
                "nodeAddress": {
                  "host": "publisher1",
                  "port": 45307,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Core",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.LargeObjects",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "name": "KlusterKite.LargeObjects.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.Console",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.ElasticSearch",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Monitoring.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.NodeManager.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Security.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLk5naW54Q29uZmlndXJhdG9yIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web.NginxConfigurator",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.NginxConfigurator 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImI1ZTE5NDI2LWJlNjYtNGUwZC1hYmJkLTU2N2QzMTk2OTBjYSJ9XQ=="
              }, "cursor": "b5e19426-be66-4e0d-abbd-567d319690ca"
            }, {
              "node": {
                "containerType": "publisher",
                "isClusterLeader": false,
                "isObsolete": false,
                "isInitialized": true,
                "leaderInRoles": [],
                "nodeId": "ba1f8a84-b069-4a0e-996f-dc55d1ba7d6f",
                "nodeTemplate": "publisher",
                "nodeTemplateVersion": 0,
                "roles": ["Web.Nginx"],
                "startTimeStamp": 636261946340758770,
                "nodeAddress": {
                  "host": "publisher2",
                  "port": 38636,
                  "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im5vZGVBZGRyZXNzIn1d"
                },
                "modules": {
                  "edges": [{
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZSAwLjAuMC4wIn1d",
                      "name": "KlusterKite.Core",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Core 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.LargeObjects",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTGFyZ2VPYmplY3RzLkNsaWVudCAwLjAuMC4wIn1d",
                      "name": "KlusterKite.LargeObjects.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.LargeObjects.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.Console",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.Console 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2ggMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Log.ElasticSearch",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Log.ElasticSearch 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQgMC4wLjAuMCJ9XQ==",
                      "name": "KlusterKite.Monitoring.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Monitoring.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.NodeManager.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.NodeManager.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuQ2xpZW50IDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Security.Client",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Security.Client 0.0.0.0"
                  }, {
                    "node": {
                      "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9LHsiZiI6Im1vZHVsZXMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLk5naW54Q29uZmlndXJhdG9yIDAuMC4wLjAifV0=",
                      "name": "KlusterKite.Web.NginxConfigurator",
                      "version": "0.0.0.0"
                    }, "cursor": "KlusterKite.Web.NginxConfigurator 0.0.0.0"
                  }]
                },
                "id": "W3siZiI6Im5vZGVNYW5hZ2VyRGF0YSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6ImJhMWY4YTg0LWIwNjktNGEwZS05OTZmLWRjNTVkMWJhN2Q2ZiJ9XQ=="
              }, "cursor": "ba1f8a84-b069-4a0e-996f-dc55d1ba7d6f"
            }]
          }
        },
        "id": "W10="
      }
    }
  };
};
