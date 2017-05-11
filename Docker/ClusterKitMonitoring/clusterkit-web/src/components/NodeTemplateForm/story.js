import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import NodeTemplateForm from './NodeTemplateForm';

storiesOf('Templates')
  .add('edit', () => {
    const template = getTemplate().data.api.template;
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <NodeTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} />;
  })
  .add('edit saving', () => {
    const template = getTemplate();
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <NodeTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} saving={true} />;
  })
  .add('edit saved', () => {
    const template = getTemplate();
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <NodeTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} saved={true} />;
  })
  .add('edit save error', () => {
    const template = getTemplate();
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <NodeTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} saveError="Cannot update this record!" />;
  })
  .add('create', () => {
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <NodeTemplateForm onSubmit={action('submitted')} packagesList={packages} />;
  })
;

const getTemplate = () => {
  return {
    "data": {
      "api": {
        "id": "W10=",
        "__typename": "ClusterKitMonitoring_ClusterKitNodeApi",
        "template": {
          "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifV0=",
          "__typename": "ClusterKitNodeApi_Template",
          "code": "publisher",
          "configuration": "{\n  ClusterKit {\n    Web {\n      Nginx {\n        PathToConfig = \"/etc/nginx/sites-enabled/clusterkit.config\"\n        ReloadCommand {\n          Command = /etc/init.d/nginx\n          Arguments = reload\n        } \n        Configuration {\n          default {\n            \"location /clusterkit\" { \n              proxy_pass = \"http://monitoringUI/clusterkit\"\n            }\n          }\n        }\n      }\n    }\n  }\n\n  akka {\n    remote {\n      helios {\n        tcp {\n          hostname = 0.0.0.0\n          port = 0\n        }\n      }\n    }\n    cluster {\n      seed-nodes = []\n    }\n  }\n}\n",
          "containerTypes": ["publisher"],
          "maximumNeededInstances": 1,
          "minimumRequiredInstances": 1,
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
              }, "cursor": "ClusterKit.Web.NginxConfigurator", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
            }, {
              "node": {
                "__id": "ClusterKit.NodeManager.Client",
                "specificVersion": null,
                "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0Lk5vZGVNYW5hZ2VyLkNsaWVudCJ9XQ==",
                "__typename": "ClusterKitNodeApi_PackageRequirement"
              }, "cursor": "ClusterKit.NodeManager.Client", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
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
              }, "cursor": "ClusterKit.Log.ElasticSearch", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
            }, {
              "node": {
                "__id": "ClusterKit.Monitoring.Client",
                "specificVersion": null,
                "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjF9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJub2RlVGVtcGxhdGVzIiwiaWQiOiJwdWJsaXNoZXIifSx7ImYiOiJwYWNrYWdlUmVxdWlyZW1lbnRzIiwiaWQiOiJDbHVzdGVyS2l0Lk1vbml0b3JpbmcuQ2xpZW50In1d",
                "__typename": "ClusterKitNodeApi_PackageRequirement"
              }, "cursor": "ClusterKit.Monitoring.Client", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
            }], "__typename": "ClusterKitNodeApi_PackageRequirement_Connection"
          },
          "priority": 1000.0
        }
      }
    }
  };
};

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
                  "name": "ClusterKit.Web.NginxConfigurator",
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
                  "name": "ClusterKit.NodeManager.Client",
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
            ]
          }
        }
      }
    }
  }
};
