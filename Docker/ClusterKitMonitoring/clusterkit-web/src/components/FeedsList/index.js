import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';
import FeedForm from '../FeedForm/index';

class FeedList extends React.Component {
  constructor (props) {
    super(props);
    this.state = {
      isEditing: false,
    }
  }

  static propTypes = {
    releaseId: React.PropTypes.string,
    configuration: React.PropTypes.object,
    onChange: React.PropTypes.func,
  };

  render() {
    const feeds = this.props.configuration.nugetFeeds && this.props.configuration.nugetFeeds.edges;

    return (
      <div>
        {!this.state.isEditing &&
          <div>
            <h2>Nuget feeds list</h2>
            <Link to={`/clusterkit/NugetFeeds/${this.props.releaseId}/create`} className="btn btn-primary" role="button">Add a new feed</Link>
            <table className="table table-hover">
              <thead>
                <tr>
                  <th>Address</th>
                  <th>Type</th>
                </tr>
              </thead>
              <tbody>
              {feeds && feeds.length > 0 && feeds.map((item) =>
                <tr key={item.node.id || item.node.address}>
                  <td>
                    <Link to={`/clusterkit/NugetFeeds/${this.props.releaseId}/${encodeURIComponent(item.node.id)}`}>
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
        }
        {this.state.isEditing &&
          <div>
            <FeedForm initialValues={this.state.editedObject} onSubmit={(model) => this.createOrUpdate(model)} />
          </div>
        }
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
              __id
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
