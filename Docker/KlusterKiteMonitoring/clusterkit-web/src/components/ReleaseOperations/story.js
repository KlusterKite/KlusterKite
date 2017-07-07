import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
// import StubContainer from 'react-storybooks-relay-container';

import CheckReleaseResult from './CheckReleaseResult';

storiesOf('Release')
  .add('check release, empty compatible', () => {
    return <CheckReleaseResult
      activeNodes={getActiveNodes().edges}
      compatibleTemplates={getCompatibleTemplatesEmpty().edges}
      newNodeTemplates={getNewNodeTemplates()}
      newReleaseInnerId={2}
    />;
  })
  .add('check release, with compatible', () => {
    return <CheckReleaseResult
      activeNodes={getActiveNodes().edges}
      compatibleTemplates={getCompatibleTemplatesFull().edges}
      newNodeTemplates={getNewNodeTemplates()}
      newReleaseInnerId={2}
    />;
  })
;

// const availableNodes = checkReleaseResult.data.clusterKitNodeApi_clusterKitNodesApi_releases_check.api.clusterKitNodesApi.getActiveNodeDescriptions;
// const compatibleTemplates = checkReleaseResult.data.clusterKitNodeApi_clusterKitNodesApi_releases_check.node.compatibleTemplates;


const getCompatibleTemplatesEmpty = () => {
  return {"edges": []};
};

const getCompatibleTemplatesFull = () => {
  return {"edges": [
    {
      "node" : {
        "templateCode": "publisher",
        "releaseId": 2,
        "compatibleReleaseId": 1
      }
    },
    {
      "node" : {
        "templateCode": "clusterManager",
        "releaseId": 2,
        "compatibleReleaseId": 1
      }
    }
  ]};
};

const getActiveNodes = () => {
  return {
    "edges": [{
      "node": {
        "nodeTemplate": null,
        "releaseId": 0,
        "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6Ijc5OWU5M2FjLTEyMzAtNDM2OC1iMDE3LTFjMjhiYjAzMjMwNiJ9XQ=="
      }, "cursor": "799e93ac-1230-4368-b017-1c28bb032306"
    }, {
      "node": {
        "nodeTemplate": "clusterManager",
        "releaseId": 1,
        "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjIxYmI3YTE0LTk1ZTgtNGFlNS05Mzc5LWZjNWU5ODJhZWQ5ZiJ9XQ=="
      }, "cursor": "21bb7a14-95e8-4ae5-9379-fc5e982aed9f"
    }, {
      "node": {
        "nodeTemplate": "publisher",
        "releaseId": 1,
        "id": "W3siZiI6ImNsdXN0ZXJLaXROb2Rlc0FwaSJ9LHsiZiI6ImdldEFjdGl2ZU5vZGVEZXNjcmlwdGlvbnMiLCJpZCI6IjdiNmM3NzljLTg3ODctNDJjOC1iNzE5LTMxMTlmMjkxODI2NSJ9XQ=="
      }, "cursor": "7b6c779c-8787-42c8-b719-3119f2918265"
    }, {
      "node": {
        "nodeTemplate": "publisher",
        "releaseId": 1,
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