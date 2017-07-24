import React from 'react';
import Relay from 'react-relay'

export class NotInSourcePositionWarning extends React.Component {
  render() {
    let warning = false;
    if (this.props.resourceState &&
      this.props.resourceState.migrationState &&
      this.props.resourceState.migrationState.templateStates &&
      this.props.resourceState.migrationState.templateStates.edges) {

      const templateStatesNodes = this.props.resourceState.migrationState.templateStates.edges.map(x => x.node);
      if (templateStatesNodes) {
        templateStatesNodes.forEach((templateStatesNode) => {
          const templateStatesNodesMigratorsNodes = templateStatesNode.migrators.edges.map(x => x.node);
          if (templateStatesNodesMigratorsNodes) {
            console.log('templateStatesNodesMigratorsNodes', templateStatesNodesMigratorsNodes);
            if (templateStatesNodesMigratorsNodes.some(node => node.position !== 'Source' && node.position !== 'SourceAndDestination')){
              warning = true;
            }
          }
        });
      }
    }

    return (
      <div>
        {warning &&
          <div className="alert alert-warning" role="alert">
            <span className="glyphicon glyphicon-alert" aria-hidden="true"></span>
            {' '}
            At least one resource position is not in Source position!
          </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  NotInSourcePositionWarning,
  {
    fragments: {
      resourceState: () => Relay.QL`fragment on IKlusterKiteNodeApi_ResourceState {
        migrationState {
          templateStates {
            edges {
              node {
                migrators {
                  edges {
                    node {
                      resources {
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
