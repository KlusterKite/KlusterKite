import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

class FeedList extends React.Component {
  constructor (props) {
    super(props);
    this.state = {
    }
  }

  static propTypes = {
    releaseId: React.PropTypes.string,
    configuration: React.PropTypes.object,
    canEdit: React.PropTypes.bool
  };

  render() {
    const feeds = this.props.configuration && this.props.configuration.nugetFeeds && this.props.configuration.nugetFeeds.edges;

    return (
      <div>
        <div>
          <h3>Nuget feeds list</h3>
          {this.props.canEdit &&
          <Link to={`/clusterkit/NugetFeeds/${this.props.releaseId}/create`} className="btn btn-primary" role="button">Add
            a new feed</Link>
          }
          {feeds && feeds.length > 0 &&
          <table className="table table-hover">
            <thead>
            <tr>
              <th>Address</th>
              <th>Type</th>
            </tr>
            </thead>
            <tbody>
            {feeds.map((item) =>
              <tr key={item.node.id || item.node.address}>
                <td>
                  {this.props.canEdit &&
                    <Link to={`/clusterkit/NugetFeeds/${this.props.releaseId}/${encodeURIComponent(item.node.id)}`}>
                      {item.node.address}
                    </Link>
                  }
                  {!this.props.canEdit &&
                    <span>{item.node.address}</span>
                  }
                </td>
                <td>
                  {item.node.type}
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
  FeedList,
  {
    fragments: {
      configuration: () => Relay.QL`fragment on IClusterKitNodeApi_ReleaseConfiguration {
        nugetFeeds {
          edges {
            node {
              id
              address
              type
              userName
              password
            }
          }
        }
      }
      `,
    },
  },
)
