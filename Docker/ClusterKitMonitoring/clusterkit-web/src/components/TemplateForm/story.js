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