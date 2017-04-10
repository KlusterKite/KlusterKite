import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

import DateFormat from '../../utils/date';

export class ReleasesList extends React.Component {
  //
  // constructor(props) {
  //   super(props);
  // }

  static propTypes = {
    clusterKitNodesApi: React.PropTypes.object,
  };

  render() {
    if (!this.props.clusterKitNodesApi.releases){
      return (<div></div>);
    }
    const edges = this.props.clusterKitNodesApi.releases.edges;

    return (
      <div>
        <h3>Releases list</h3>
        <Link to={`/clusterkit/Releases/create`} className="btn btn-primary" role="button">Add a new release</Link>
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Name</th>
              <th>Created</th>
              <th>State</th>
            </tr>
          </thead>
          <tbody>
          {edges && edges.map((edge) => {
            const node = edge.node;
            const dateCreated = new Date(node.created);

            return (
              <tr key={`${node.id}`}>
                <td>
                  <Link to={`/clusterkit/Releases/${encodeURIComponent(node.id)}`}>
                    {node.name}
                  </Link>
                </td>
                <td>{DateFormat.formatDateTime(dateCreated)}</td>
                <td>{node.state}</td>
              </tr>
            )
          })
          }
          </tbody>
        </table>

      </div>
    );
  }
}

export default Relay.createContainer(
  ReleasesList,
  {
    fragments: {
      clusterKitNodesApi: () => Relay.QL`fragment on IClusterKitNodeApi_Root {
        releases {
          edges {
            node {
              id
              name
              notes
              minorVersion
              majorVersion
              created
              started
              finished
              state
              isStable
            }
          }
        }
      }
      `,
    },
  },
)
