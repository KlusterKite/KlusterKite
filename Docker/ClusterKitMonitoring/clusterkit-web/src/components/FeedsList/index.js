import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

class FeedList extends React.Component {

  static propTypes = {
    feeds: React.PropTypes.object,
  };

  render() {
    const feeds = this.props.feeds.nugetFeeds && this.props.feeds.nugetFeeds.edges;

    return (
      <div>
        <h2>Nuget feeds list</h2>
        <Link to="/NugetFeeds/create" className="btn btn-primary" role="button">Add a new feed</Link>
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Address</th>
              <th>Type</th>
            </tr>
          </thead>
          <tbody>
          {feeds && feeds.length > 0 && feeds.map((item) =>
            <tr key={item.node.id}>
              <td>
                <Link to={`/NugetFeeds/${encodeURIComponent(item.node.id)}`}>
                {item.node.address}
                </Link>
              </td>
              <td>
                {item.node.type}
              </td>
            </tr>
          )
          }
          </tbody>
        </table>
      </div>
    );
  }
}

export default Relay.createContainer(
  FeedList,
  {
    initialVariables: {
      filter: {
        address_not: "user@email"
      }
    },
    fragments: {
      feeds: () => Relay.QL`fragment on IClusterKitNodeApi_ClusterKitNodeManagement {
        nugetFeeds(filter: $filter ) {
          edges {
            node {
              id
              address
              type
            }
          }
        }
      }
      `,
    },
  },
)
