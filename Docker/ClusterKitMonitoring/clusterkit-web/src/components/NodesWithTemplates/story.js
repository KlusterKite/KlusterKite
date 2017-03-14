import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import NodesWithTemplates from './index';

storiesOf('Homepage')
  .add('nodes with templates', () => {
    const data = getData();

    const props = {
      data: data.data.api.nodeManagerData,
      hasError: false,
      upgradeNodePrivilege: true,
      onManualUpgrade: action('onManualUpgrade')
    };
    return <StubContainer Component={NodesWithTemplates} props={props} />;
  })
;

// const getActiveNodeDescriptions = function () {
//   return [
//     {
//       "NodeTemplate": null,
//     },
//     {
//       "NodeTemplate": "clusterManager",
//     },
//     {
//       "NodeTemplate": "clusterManager",
//     },
//     {
//       "NodeTemplate": "publisher",
//     },
//     {
//       "NodeTemplate": "publisher",
//     }
//   ];
// };
//
// const getNodeTemplatesList = function () {
//   return [
//     {
//       "Code": "clusterManager",
//       "MinimumRequiredInstances": 1,
//       "Name": "Cluster manager (cluster monitoring and managing)"
//     },
//     {
//       "Code": "empty",
//       "MinimumRequiredInstances": 0,
//       "Name": "Cluster empty instance, just for demo"
//     },
//     {
//       "Code": "publisher",
//       "MinimumRequiredInstances": 1,
//       "Name": "Cluster Nginx configurator"
//     }
//   ];
// };


const getData = () => {
  return {
    "data": {
      "api": {
        "nodeManagerData": {
          "getActiveNodeDescriptions": [{"nodeTemplate": null}, {"nodeTemplate": "clusterManager"}, {"nodeTemplate": "clusterManager"}, {"nodeTemplate": "publisher"}, {"nodeTemplate": "publisher"}],
          "nodeTemplates": {
            "edges": [{
              "node": {
                "code": "publisher",
                "minimumRequiredInstances": 1,
                "name": "Cluster Nginx configurator",
                "id": 1
              }, "cursor": 1
            }, {
              "node": {
                "code": "clusterManager",
                "minimumRequiredInstances": 1,
                "name": "Cluster manager (cluster monitoring and managing)",
                "id": 2
              }, "cursor": 2
            }, {
              "node": {
                "code": "empty",
                "minimumRequiredInstances": 0,
                "name": "Cluster empty instance, just for demo",
                "id": 3
              }, "cursor": 3
            }]
          }
        }
      }
    }
  };
};
