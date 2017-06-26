import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import MigratorTemplateForm from './MigratorTemplateForm';

storiesOf('Migrator Templates')
  .add('edit', () => {
    const template = getTemplate().data.api.template;
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <MigratorTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} />;
  })
  .add('edit saving', () => {
    const template = getTemplate().data.api.template;
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <MigratorTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} saving={true} />;
  })
  .add('edit saved', () => {
    const template = getTemplate().data.api.template;
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <MigratorTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} saved={true} />;
  })
  .add('edit save error', () => {
    const template = getTemplate().data.api.template;
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <MigratorTemplateForm onSubmit={action('submitted')} initialValues={template} packagesList={packages} saveError="Cannot update this record!" />;
  })
  .add('create', () => {
    const packages = getPackages().data.api.clusterKitNodesApi.nugetPackages;
    return <MigratorTemplateForm onSubmit={action('submitted')} packagesList={packages} />;
  })
;

const getTemplate = () => {
  return {
    "data": {
      "api": {
        "template": {
          "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjJ9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJtaWdyYXRvclRlbXBsYXRlcyIsImlkIjoiQ2x1c3RlcktpdCJ9XQ==",
          "__typename": "ClusterKitNodeApi_MigratorTemplate",
          "code": "ClusterKit",
          "configuration": "{\n\tClusterKit.NodeManager.ConfigurationDatabaseConnectionString = \"User ID=postgres;Host=configDb;Port=5432;Pooling=true;Database=ClusterKit.NodeManagerConfiguration\"\t  \n    ClusterKit.NodeManager.Migrators = [\n        \"ClusterKit.NodeManager.ConfigurationSource.Migrator.ConfigurationMigrator, ClusterKit.NodeManager.ConfigurationSource.Migrator\"\n    ]\n}\n",
          "name": "ClusterKit Migrator",
          "notes": null,
          "packageRequirements": {
            "edges": [{
              "node": {
                "__id": "ClusterKit.NodeManager.ConfigurationSource.Migrator",
                "specificVersion": null,
                "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjJ9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJtaWdyYXRvclRlbXBsYXRlcyIsImlkIjoiQ2x1c3RlcktpdCJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIuQ29uZmlndXJhdGlvblNvdXJjZS5NaWdyYXRvciJ9XQ==",
                "__typename": "ClusterKitNodeApi_PackageRequirement"
              },
              "cursor": "ClusterKit.NodeManager.ConfigurationSource.Migrator",
              "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
            }, {
              "node": {
                "__id": "ClusterKit.NodeManager",
                "specificVersion": null,
                "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjJ9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJtaWdyYXRvclRlbXBsYXRlcyIsImlkIjoiQ2x1c3RlcktpdCJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuTm9kZU1hbmFnZXIifV0=",
                "__typename": "ClusterKitNodeApi_PackageRequirement"
              }, "cursor": "ClusterKit.NodeManager", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
            }, {
              "node": {
                "__id": "ClusterKit.Data.EF.Npgsql",
                "specificVersion": null,
                "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6InJlbGVhc2VzIiwiaWQiOjJ9LHsiZiI6ImNvbmZpZ3VyYXRpb24ifSx7ImYiOiJtaWdyYXRvclRlbXBsYXRlcyIsImlkIjoiQ2x1c3RlcktpdCJ9LHsiZiI6InBhY2thZ2VSZXF1aXJlbWVudHMiLCJpZCI6IkNsdXN0ZXJLaXQuRGF0YS5FRi5OcGdzcWwifV0=",
                "__typename": "ClusterKitNodeApi_PackageRequirement"
              }, "cursor": "ClusterKit.Data.EF.Npgsql", "__typename": "ClusterKitNodeApi_PackageRequirement_Edge"
            }], "__typename": "ClusterKitNodeApi_PackageRequirement_Connection"
          },
          "priority": 1.0
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
                  "name": "ClusterKit.NodeManager.ConfigurationSource.Migrator",
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
                  "name": "ClusterKit.Data.EF.Npgsql",
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
