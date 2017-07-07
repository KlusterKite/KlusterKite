import React from 'react'
import Relay from 'react-relay'

import UserList from '../../components/UsersList/UsersList';

import { hasPrivilege } from '../../utils/privileges';

class UsersListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    const canCreate = hasPrivilege('KlusterKite.NodeManager.User.Create');
    const canEdit = hasPrivilege('KlusterKite.NodeManager.User.Update');

    return (
      <div>
        <UserList
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
          ${UserList.getFragment('klusterKiteNodesApi')},
        }
      }
      `,
    }
  },
)
