import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import CheckConfigurationResult from './CheckConfigurationResult';

storiesOf('Configuration')
  .add('check configuration, empty compatible', () => {
    return <CheckConfigurationResult
      activeNodes={getActiveNodes().edges}
      compatibleTemplates={getCompatibleTemplatesEmpty().edges}
      newNodeTemplates={getNewNodeTemplates()}
      newconfigurationInnerId={2}
    />;
  })
  .add('check configuration, with compatible', () => {
    return <CheckConfigurationResult
      activeNodes={getActiveNodes().edges}
      compatibleTemplates={getCompatibleTemplatesFull().edges}
      newNodeTemplates={getNewNodeTemplates()}
      newconfigurationInnerId={2}
    />;
  })
;

// const availableNodes = checkConfigurationResult.data.klusterKiteNodeApi_klusterKiteNodesApi_configurations_check.api.klusterKiteNodesApi.getActiveNodeDescriptions;
// const compatibleTemplates = checkConfigurationResult.data.klusterKiteNodeApi_klusterKiteNodesApi_configurations_check.node.compatibleTemplates;


const getCompatibleTemplatesEmpty = () => {
  return {"edges": []};
};

const getCompatibleTemplatesFull = () => {
  return {"edges": [
    {
      "node" : {
        "templateCode": "publisher",
        "configurationId": 2,
        "compatibleconfigurationId": 1
      }
    },
    {
      "node" : {
        "templateCode": "clusterManager",
        "configurationId": 2,
        "compatibleconfigurationId": 1
      }
    }
  ]};
};

const getActiveNodes = () => {
  return {
    "edges": [{
      "node": {
        "nodeTemplate": null,
        "configurationId": 0,
        "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6Ijc5OWU5M2FjLTEyMzAtNDM2OC1iMDE3LTFjMjhiYjAzMjMwNiJ9XQ=="
      }, "cursor": "799e93ac-1230-4368-b017-1c28bb032306"
    }, {
      "node": {
        "nodeTemplate": "clusterManager",
        "configurationId": 1,
        "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjIxYmI3YTE0LTk1ZTgtNGFlNS05Mzc5LWZjNWU5ODJhZWQ5ZiJ9XQ=="
      }, "cursor": "21bb7a14-95e8-4ae5-9379-fc5e982aed9f"
    }, {
      "node": {
        "nodeTemplate": "publisher",
        "configurationId": 1,
        "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjdiNmM3NzljLTg3ODctNDJjOC1iNzE5LTMxMTlmMjkxODI2NSJ9XQ=="
      }, "cursor": "7b6c779c-8787-42c8-b719-3119f2918265"
    }, {
      "node": {
        "nodeTemplate": "publisher",
        "configurationId": 1,
        "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjkxZGNjMzE0LWYxYzAtNGJlYy05NWI1LTM3YjgyNjA2OGVhOSJ9XQ=="
      }, "cursor": "91dcc314-f1c0-4bec-95b5-37b826068ea9"
    }]
  };
};

const getNewNodeTemplates = () => {
  return [
    {
      "node": {
        "name": "Cluster Nginx configurator",
        "code": "publisher"
      }
    },
    {
      "node": {
        "name": "Cluster manager (cluster monitoring and managing)",
        "code": "clusterManager"
      }
    },
    {
      "node": {
        "name": "Cluster empty instance, just for demo",
        "code": "empty"
      }
    }
  ]
};