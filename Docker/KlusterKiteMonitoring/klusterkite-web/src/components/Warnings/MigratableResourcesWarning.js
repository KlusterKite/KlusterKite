import React from 'react';
import Relay from 'react-relay'

export class MigratableResourcesWarning extends React.Component {
  render() {
    const resourceState = this.props.resourceState;

    return (
      <div>
        {resourceState && resourceState.configurationState.migratableResources.edges.length > 0 &&
          <div className="alert alert-warning" role="alert">
            <span className="glyphicon glyphicon-alert" aria-hidden="true"></span>
            {' '}
            Migratable Resources found.
          </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  MigratableResourcesWarning,
  {
    fragments: {
      resourceState: () => Relay.QL`fragment on IKlusterKiteNodeApi_ResourceState {
        configurationState {
          migratableResources {
            edges {
              node {
                position
              }
            }
          }
        }
      }
      `,
    },
  },
)
