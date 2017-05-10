import React from 'react'
import Relay from 'react-relay'

import RolesList from '../../components/RolesList/RolesList';

import { hasPrivilege } from '../../utils/privileges';

class UsersListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    const canCreate = hasPrivilege('ClusterKit.NodeManager.Role.Create');
    const canEdit = hasPrivilege('ClusterKit.NodeManager.Role.Update');

    return (
      <div>
        <RolesList
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
          ${RolesList.getFragment('clusterKitNodesApi')},
        }
      }
      `,
    }
  },
)
