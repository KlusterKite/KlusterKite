import React from 'react'
import Relay from 'react-relay'

import ReleasesList from '../../components/ReleasesList/ReleasesList';

class ReleasesListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    return (
      <div>
        <ReleasesList clusterKitNodesApi={this.props.api.clusterKitNodesApi} />
      </div>
    )
  }
}

// ${FeedsListOld.getFragment('feeds')},
export default Relay.createContainer(
  ReleasesListPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on IClusterKitNodeApi {
        __typename
        clusterKitNodesApi {
          ${ReleasesList.getFragment('clusterKitNodesApi')},
        }
      }
      `,
    }
  },
)
