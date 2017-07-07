import React from 'react'
import Relay from 'react-relay'

import RolesList from '../../components/RolesList/RolesList';

import { hasPrivilege } from '../../utils/privileges';

class UsersListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    const canCreate = hasPrivilege('KlusterKite.NodeManager.Role.Create');
    const canEdit = hasPrivilege('KlusterKite.NodeManager.Role.Update');

    return (
      <div>
        <RolesList
          klusterKiteNodesApi={this.props.api.klusterKiteNodesApi}
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
      api: () => Relay.QL`fragment on IKlusterKiteNodeApi {
        __typename
        klusterKiteNodesApi {
          ${RolesList.getFragment('klusterKiteNodesApi')},
        }
      }
      `,
    }
  },
)
