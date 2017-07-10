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
    configurationId: React.PropTypes.string,
    configuration: React.PropTypes.object,
    canEdit: React.PropTypes.bool
  };

  render() {
    return (
      <div>
        <div>
          <h3>Nuget feeds</h3>

          <p>
            {this.props.canEdit &&
              <Link to={`/klusterkite/NugetFeeds/${this.props.configurationId}`}>
                {this.props.configuration && this.props.configuration.nugetFeed}
              </Link>
            }
            {!this.props.canEdit &&
              <span>{this.props.configuration && this.props.configuration.nugetFeed}</span>
            }
          </p>
        </div>
      </div>
    );
  }
}

export default Relay.createContainer(
  FeedList,
  {
    fragments: {
      configuration: () => Relay.QL`fragment on IKlusterKiteNodeApi_ConfigurationSettings {
        nugetFeed
      }
      `,
    },
  },
)
