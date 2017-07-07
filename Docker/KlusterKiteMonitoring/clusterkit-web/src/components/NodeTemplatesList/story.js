import React from 'react';
import { storiesOf } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import NodeTemplatesList from './NodeTemplatesList';

storiesOf('Node Templates')
  .add('list, full access', () => {
    const node = getTemplatesListRelay();
    const props = {
      configuration: node.configuration,
      canEdit: true,
    };
    console.log(props);
    return <StubContainer Component={NodeTemplatesList} props={props} />;
  })
  .add('list, no access to edit', () => {
    const node = getTemplatesListRelay();
    const props = {
      configuration: node.configuration,
      canEdit: false,
    };
    console.log(props);
    return <StubContainer Component={NodeTemplatesList} props={props} />;
  })
;

const getTemplatesListRelay = () => {
  return {
    "configuration": {
      "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifV0=",
      "__typename": "ClusterKitNodeApi_ReleaseConfiguration",
      "nodeTemplates": {
        "edges": [{
          "node": {
            "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifV0=",
            "code": "publisher",
            "configuration": "{\n  ClusterKit {\n    Web {\n      Nginx {\n        PathToConfig = \"/etc/nginx/sites-enabled/clusterkit.config\"\n        ReloadCommand {\n          Command = /etc/init.d/nginx\n          Arguments = reload\n        } \n        Configuration {\n          default {\n            \"location /clusterkit\" { \n              proxy_pass = \"http://monitoringUI/clusterkit\"\n            }\n          }\n        }\n      }\n    }\n  }\n\n  akka {\n    remote {\n      helios {\n        tcp {\n          hostname = 0.0.0.0\n          port = 0\n        }\n      }\n    }\n    cluster {\n      seed-nodes = []\n    }\n  }\n}\n",
            "containerTypes": ["publisher"],
            "minimumRequiredInstances": 1,
            "maximumNeededInstances": null,
            "name": "Cluster Nginx configurator",
            "packageRequirements": {
              "edges": [{
                "node": {
                  "__id": "ClusterKit.Core.Service",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0LkNvcmUuU2VydmljZSJ9XQ==",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.Core.Service", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Web.NginxConfigurator",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0LldlYi5OZ2lueENvbmZpZ3VyYXRvciJ9XQ==",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Web.NginxConfigurator",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.NodeManager.Client",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0Lk5vZGVNYW5hZ2VyLkNsaWVudCJ9XQ==",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.NodeManager.Client",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Log.Console",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0LkxvZy5Db25zb2xlIn1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.Log.Console", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Log.ElasticSearch",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0LkxvZy5FbGFzdGljU2VhcmNoIn1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Log.ElasticSearch",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Monitoring.Client",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0Lk1vbml0b3JpbmcuQ2xpZW50In1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Monitoring.Client",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }], "__typename": "ClusterKitNodeApi_PackageRequirement_Connection"
            },
            "priority": 1000.0,
            "__typename": "ClusterKitNodeApi_NodeTemplate"
          }, "cursor": "publisher", "__typename": "ClusterKitNodeApi_NodeTemplate_Edge"
        }, {
          "node": {
            "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9XQ==",
            "code": "clusterManager",
            "configuration": "{\n  ClusterKit {\n\n    NodeManager.ConfigurationDatabaseConnectionString = \"User ID=postgres;Host=configDb;Port=5432;Pooling=true\"\n      \n    Web {\n\n      Swagger.Publish {\n          publishDocPath = \"\"clusterkit/manager/swagger/doc\"\"\n          publishUiPath = \"\"clusterkit/manager/swagger/ui\"\"\n      }\n\n      Services {\n        ClusterKit/Web/Swagger { // ServiceName is just unique service identification, used in order to handle stacked config properly. It is used just localy on node\n          Port = 8080 // default owin port, current node listening port for server access\n          PublicHostName = default //public host name of this service. It doesn't supposed (but is not prohibited) that this should be real public service hostname. It's just used to distinguish services with identical url paths to be correctly published on frontend web servers. Real expected hostname should be configured in NginxConfigurator or similar publisher\n          Route = /clusterkit/manager/swagger //route (aka directory) path to service\n        }                    \n      }\n\n    }\n  }\n\n\n\n  akka {\n    remote {\n      helios {\n        tcp {\n          hostname = 0.0.0.0\n          port = 0\n        }\n      }\n    }\n    cluster {\n      seed-nodes = []\n    }\n  }\n}\n",
            "containerTypes": ["manager", "worker"],
            "minimumRequiredInstances": 1,
            "maximumNeededInstances": 3,
            "name": "Cluster manager (cluster monitoring and managing)",
            "packageRequirements": {
              "edges": [{
                "node": {
                  "__id": "ClusterKit.Core.Service",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZS5TZXJ2aWNlIn1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.Core.Service", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.NodeManager.Client",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50In1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.NodeManager.Client",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Monitoring.Client",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQifV0=",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Monitoring.Client",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Monitoring",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZyJ9XQ==",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.Monitoring", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.NodeManager",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIifV0=",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.NodeManager", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Data.EF.Npgsql",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRi5OcGdzcWwifV0=",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Data.EF.Npgsql",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Web.Swagger.Monitor",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIuTW9uaXRvciJ9XQ==",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Web.Swagger.Monitor",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Web.Swagger",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLlN3YWdnZXIifV0=",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.Web.Swagger", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Log.Console",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkNvbnNvbGUifV0=",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.Log.Console", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Log.ElasticSearch",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTG9nLkVsYXN0aWNTZWFyY2gifV0=",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Log.ElasticSearch",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Web.Authentication",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkF1dGhlbnRpY2F0aW9uIn1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Web.Authentication",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Security.SessionRedis",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuU2VjdXJpdHkuU2Vzc2lvblJlZGlzIn1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Security.SessionRedis",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.API.Endpoint",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuQVBJLkVuZHBvaW50In1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.API.Endpoint", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Web.GraphQL.Publisher",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJjbHVzdGVyTWFuYWdlciJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuV2ViLkdyYXBoUUwuUHVibGlzaGVyIn1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Web.GraphQL.Publisher",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }], "__typename": "ClusterKitNodeApi_PackageRequirement_Connection"
            },
            "priority": 100.0,
            "__typename": "ClusterKitNodeApi_NodeTemplate"
          }, "cursor": "clusterManager", "__typename": "ClusterKitNodeApi_NodeTemplate_Edge"
        }, {
          "node": {
            "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJlbXB0eSJ9XQ==",
            "code": "empty",
            "configuration": "{\n  ClusterKit {\n\n    Web {\n\n      Services {\n        //ClusterKit/Web/Swagger { // ServiceName is just unique service identification, used in order to handle stacked config properly. It is used just localy on node\n          //Port = 8080 // default owin port, current node listening port for server access\n          //PublicHostName = default //public host name of this service. It doesn't supposed (but is not prohibited) that this should be real public service hostname. It's just used to distinguish services with identical url paths to be correctly published on frontend web servers. Real expected hostname should be configured in NginxConfigurator or similar publisher\n          //Route = /clusterkit/manager/swagger //route (aka directory) path to service\n        //}                    \n      }\n\n    }\n  }\n\n\n\n  akka {\n    remote {\n      helios {\n        tcp {\n          hostname = 0.0.0.0\n          port = 0\n        }\n      }\n    }\n    cluster {\n      seed-nodes = []\n    }\n  }\n}\n",
            "containerTypes": ["worker"],
            "minimumRequiredInstances": 0,
            "maximumNeededInstances": null,
            "name": "Cluster empty instance, just for demo",
            "packageRequirements": {
              "edges": [{
                "node": {
                  "__id": "ClusterKit.Core.Service",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJlbXB0eSJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuQ29yZS5TZXJ2aWNlIn1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                }, "cursor": "ClusterKit.Core.Service", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.NodeManager.Client",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJlbXB0eSJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ2xpZW50In1d",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.NodeManager.Client",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }, {
                "node": {
                  "__id": "ClusterKit.Monitoring.Client",
                  "specificVersion": null,
                  "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJlbXB0eSJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTW9uaXRvcmluZy5DbGllbnQifV0=",
                  "__typename": "ClusterKitNodeApi_PackageRequirement"
                },
                "cursor": "ClusterKit.Monitoring.Client",
                "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
              }], "__typename": "ClusterKitNodeApi_PackageRequirement_Connection"
            },
            "priority": 1.0,
            "__typename": "ClusterKitNodeApi_NodeTemplate"
          }, "cursor": "empty", "__typename": "ClusterKitNodeApi_NodeTemplate_Edge"
        }], "__typename": "ClusterKitNodeApi_NodeTemplate_Connection"
      }
    }
  };
};
