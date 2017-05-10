import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

class RolesList extends React.Component {
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
    const users = this.props.clusterKitNodesApi && this.props.clusterKitNodesApi.roles;

    return (
      <div>
        <div>
          <h3>Roles list</h3>
          {this.props.canCreate &&
            <Link to={`/clusterkit/Roles/create`} className="btn btn-primary" role="button">Add a new
              role</Link>
          }
          {users && users.edges.length > 0 &&
          <table className="table table-hover">
            <thead>
            <tr>
              <th>Name</th>
            </tr>
            </thead>
            <tbody>
            {users.edges.map((item) =>
              <tr key={item.node.id}>
                <td>
                  {this.props.canEdit &&
                    <Link to={`/clusterkit/Roles/${item.node.id}`}>
                      {item.node.name}
                    </Link>
                  }
                  {!this.props.canEdit &&
                    <span>{item.node.name}</span>
                  }
                </td>
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
  RolesList,
  {
    fragments: {
      clusterKitNodesApi: () => Relay.QL`fragment on IClusterKitNodeApi_Root {
        roles {
          edges {
            node {
              id
              name
            }
          }
        }
      }
      `,
    },
  },
)
