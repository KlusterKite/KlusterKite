import React from 'react';
import Relay from 'react-relay'

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
    return (
      <div>
        <div>
          <h3>Nuget feeds</h3>

          <p>{this.props.configuration && this.props.configuration.nugetFeed}</p>
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
        nugetFeed
      }
      `,
    },
  },
)
