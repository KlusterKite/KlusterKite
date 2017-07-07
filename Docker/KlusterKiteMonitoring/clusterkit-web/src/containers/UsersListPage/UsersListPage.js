import React from 'react'
import Relay from 'react-relay'

import UserList from '../../components/UsersList/UsersList';

import { hasPrivilege } from '../../utils/privileges';

class UsersListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    const canCreate = hasPrivilege('ClusterKit.NodeManager.User.Create');
    const canEdit = hasPrivilege('ClusterKit.NodeManager.User.Update');

    return (
      <div>
        <UserList
          clusterKitNodesApi={this.props.api.clusterKitNodesApi}
          canCreate={canCreate}
          canEdit={canEdit}
        />
      </div>
    )
  }
}

export default Relay.createContainer(
  UsersListPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on IClusterKitNodeApi {
        __typename
        clusterKitNodesApi {
          ${UserList.getFragment('clusterKitNodesApi')},
        }
      }
      `,
    }
  },
)
