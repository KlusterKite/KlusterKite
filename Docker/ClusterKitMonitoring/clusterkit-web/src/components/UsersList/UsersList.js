import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

class UsersList extends React.Component {
  constructor (props) {
    super(props);
    this.state = {
    }
  }

  static propTypes = {
    clusterKitNodesApi: React.PropTypes.object,
    canCreate: React.PropTypes.bool,
    canEdit: React.PropTypes.bool,
  };

  render() {
    const users = this.props.clusterKitNodesApi && this.props.clusterKitNodesApi.users;

    return (
      <div>
        <div>
          <h3>Users list</h3>
          {this.props.canCreate &&
            <Link to={`/clusterkit/Users/create`} className="btn btn-primary" role="button">Add a new
              user</Link>
          }
          {users && users.edges.length > 0 &&
          <table className="table table-hover">
            <thead>
            <tr>
              <th>Username</th>
              <th>Is blocked?</th>
              <th>Is deleted?</th>
            </tr>
            </thead>
            <tbody>
            {users.edges.map((item) =>
              <tr key={item.node.id}>
                <td>
                  {this.props.canEdit &&
                    <Link to={`/clusterkit/Users/${item.node.id}`}>
                      {item.node.login}
                    </Link>
                  }
                  {!this.props.canEdit &&
                    <span>{item.node.login}</span>
                  }
                </td>
                <td>{item.node.isBlocked.toString()}</td>
                <td>{item.node.isDeleted.toString()}</td>
              </tr>
            )
            }
            </tbody>
          </table>
          }
        </div>
      </div>
    );
  }
}

export default Relay.createContainer(
  UsersList,
  {
    fragments: {
      clusterKitNodesApi: () => Relay.QL`fragment on IClusterKitNodeApi_Root {
        users {
          edges {
            node {
              id
              login
              isBlocked
              isDeleted
            }
          }
        }
      }
      `,
    },
  },
)
