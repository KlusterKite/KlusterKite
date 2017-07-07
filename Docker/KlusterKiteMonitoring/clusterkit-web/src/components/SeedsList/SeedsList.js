import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

class SeedsList extends React.Component {
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
    const seeds = this.props.configuration && this.props.configuration.seedAddresses;

    return (
      <div>
        <div>
          <h3>Seed list</h3>
          {this.props.canEdit &&
            <Link to={`/clusterkit/Seeds/${this.props.releaseId}`} className="btn btn-primary" role="button">Add a new
              seed</Link>
          }
          {seeds && seeds.length > 0 &&
          <table className="table table-hover">
            <thead>
            <tr>
              <th>Address</th>
            </tr>
            </thead>
            <tbody>
            {seeds.map((item, index) =>
              <tr key={index}>
                <td>
                  {this.props.canEdit &&
                    <Link to={`/clusterkit/Seeds/${this.props.releaseId}`}>
                      {item}
                    </Link>
                  }
                  {!this.props.canEdit &&
                    <span>{item}</span>
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
  SeedsList,
  {
    fragments: {
      configuration: () => Relay.QL`fragment on IClusterKitNodeApi_ReleaseConfiguration {
        seedAddresses
      }
      `,
    },
  },
)
