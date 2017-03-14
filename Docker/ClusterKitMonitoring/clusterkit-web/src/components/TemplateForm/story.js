import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import TemplateForm from './index';

storiesOf('Templates')
  .add('edit', () => {
    const template = getTemplate();
    return <TemplateForm onSubmit={action('submitted')} initialValues={template} />;
  })
  .add('edit saving', () => {
    const template = getTemplate();
    return <TemplateForm onSubmit={action('submitted')} initialValues={template} saving={true} />;
  })
  .add('edit saved', () => {
    const template = getTemplate();
    return <TemplateForm onSubmit={action('submitted')} initialValues={template} saved={true} />;
  })
  .add('edit save error', () => {
    const template = getTemplate();
    return <TemplateForm onSubmit={action('submitted')} initialValues={template} saveError="Cannot update this record!" />;
  })
  .add('create', () => {
    return <TemplateForm onSubmit={action('submitted')} />;
  })
;

let getTemplateOld = function () {
  const template = {
    "Code": "clusterManager",
    "Configuration": "{\n  ClusterKit {\n\n    NodeManager.ConfigurationDatabaseConnectionString = \"User ID=postgres;Host=configDb;Port=5432;Pooling=true\"\n      \n    Web {\n\n      Swagger.Publish {\n          publishDocPath = \"\"clusterkit/manager/swagger/doc\"\"\n          publishUiPath = \"\"clusterkit/manager/swagger/ui\"\"\n      }\n\n      Services {\n        ClusterKit/Web/Swagger { // ServiceName is just unique service identification, used in order to handle stacked config properly. It is used just localy on node\n          Port = 8080 // default owin port, current node listening port for server access\n          PublicHostName = default //public host name of this service. It doesn't supposed (but is not prohibited) that this should be real public service hostname. It's just used to distinguish services with identical url paths to be correctly published on frontend web servers. Real expected hostname should be configured in NginxConfigurator or similar publisher\n          Route = /clusterkit/manager/swagger //route (aka directory) path to service\n        }                    \n      }\n\n    }\n  }\n\n\n\n  akka {\n    remote {\n      helios {\n        tcp {\n          hostname = 0.0.0.0\n          port = 0\n        }\n      }\n    }\n    cluster {\n      seed-nodes = []\n    }\n  }\n}\n",
    "ContainerTypes": [
      "manager",
      "worker"
    ],
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
  };
  return template;
};

const getTemplate = () => {
  return {
    "id": "H4sIAAAAAAAAA6tWKlCyiq5WSlOyUsrLT0n1TcxLTE8tckksSVSq1UGIh6TmFuQklqQWK9XG6iglFmQCxZ1zSotLUou8M0v8gCocgWI6SpkpSlaGtQA3uBnbVgAAAA==",
    "__typename": "ClusterKitNodeApi_ClusterKitNodeTemplate_Node",
    "__id": 1,
    "code": "publisher",
    "configuration": "{\n  ClusterKit {\n    Web {\n      Nginx {\n        PathToConfig = \"/etc/nginx/sites-enabled/clusterkit.config\"\n        ReloadCommand {\n          Command = /etc/init.d/nginx\n          Arguments = reload\n        } \n        Configuration {\n          default {\n            \"location /clusterkit\" { \n              proxy_pass = \"http://monitoringUI/clusterkit\"\n            }\n          }\n        }\n      }\n    }\n  }\n\n  akka {\n    remote {\n      helios {\n        tcp {\n          hostname = 0.0.0.0\n          port = 0\n        }\n      }\n    }\n    cluster {\n      seed-nodes = []\n    }\n  }\n}\n",
    "containerTypes": ["publisher"],
    "maximumNeededInstances": null,
    "minimumRequiredInstances": 1,
    "name": "Cluster Nginx configurator",
    "packages": ["ClusterKit.Core.Service", "ClusterKit.Web.NginxConfigurator", "ClusterKit.NodeManager.Client", "ClusterKit.Log.Console", "ClusterKit.Log.ElasticSearch", "ClusterKit.Monitoring.Client"],
    "priority": 1000.0,
    "version": 0
  };
}