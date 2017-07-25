import React from 'react';
import Relay from 'react-relay'

export class OutOfScopeWarning extends React.Component {
  render() {
    const resourceState = this.props.resourceState;
    let error = false;
    if (resourceState && resourceState.configurationState.states.edges) {
      const configurationStateNodes = resourceState.configurationState.states.edges.map(x => x.node);
      configurationStateNodes.forEach(configurationStateNode => {
        if (configurationStateNode.migratorsStates.edges.length > 0) {
          const migratorsStatesNodes = configurationStateNode.migratorsStates.edges.map(x => x.node);
          migratorsStatesNodes.forEach(migratorsStatesNode => {
            if (migratorsStatesNode.resources.edges.length > 0){
              error = true;
            }
          });
        }
      });
    }

    return (
      <div>
        {error &&
          <div className="alert alert-danger" role="alert">
            <span className="glyphicon glyphicon-alert" aria-hidden="true"></span>
            {' '}
            At least one resource in OutOfScope state found.
          </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  OutOfScopeWarning,
  {
    fragments: {
      resourceState: () => Relay.QL`fragment on IKlusterKiteNodeApi_ResourceState {
        configurationState {
          states {
            edges {
              node {
                migratorsStates {
                  edges {
                    node {
                      resources(filter: { position: OutOfScope }) {
                        edges {
                          node {
                            position
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      `,
    },
  },
)
