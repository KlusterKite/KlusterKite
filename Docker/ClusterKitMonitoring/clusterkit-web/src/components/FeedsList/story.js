import React from 'react';
import { storiesOf, action } from '@kadira/storybook';
import StubContainer from 'react-storybooks-relay-container';

import FeedList from './FeedList';

storiesOf('Feeds')
  .add('list', () => {
    const nodes = getFeedsListRelay();
    const props = {
      configuration: nodes.data.api.nodeManagerData,
      canEdit: true
    };
    return <StubContainer Component={FeedList} props={props} />;
  })
;

const getFeedsListRelay = () => {
  return {
    "data": {
      "api": {
        "nodeManagerData": {
          "nugetFeeds": {
            "edges": [{
              "node": {
                "address": "/opt/packageCache",
                "type": "Private",
                "id": "{\"p\":[{\"f\":\"nodeManagerData\",\"a\":{}},{\"f\":\"nugetFeeds\"}],\"api\":\"ClusterKitNodeApi\",\"id\":1}"
              }, "cursor": null
            }, {
              "node": {
                "address": "http://nuget/",
                "type": "Private",
                "id": "{\"p\":[{\"f\":\"nodeManagerData\",\"a\":{}},{\"f\":\"nugetFeeds\"}],\"api\":\"ClusterKitNodeApi\",\"id\":2}"
              }, "cursor": null
            }]
          }
        }
      }
    }
  }
};
