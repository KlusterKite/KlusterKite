import React from 'react'
import Relay from 'react-relay'

import FeedsList from '../../components/FeedsList/index';

class FeedsListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    return (
      <div>
        <FeedsList feeds={this.props.api.nodeManagerData} />
      </div>
    )
  }
}

// ${FeedsList.getFragment('feeds')},
export default Relay.createContainer(
  FeedsListPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on ClusterKitMonitoring_ClusterKitNodeApi {
        __typename
        nodeManagerData {
          ${FeedsList.getFragment('feeds')},
        }
      }
      `,
    }
  },
)
