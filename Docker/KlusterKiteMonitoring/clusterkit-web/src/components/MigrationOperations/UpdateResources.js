import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import UpdateResourcesMutation from './mutations/UpdateResourcesMutation';

import './styles.css';

export class UpdateResources extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isProcessing: false,
      processSuccessful: false,
      processErrors: null,
    };
  }

  static propTypes = {
    migrationState: React.PropTypes.object,
    onStateChange: React.PropTypes.func.isRequired,
    onError: React.PropTypes.func.isRequired,
    operationIsInProgress: React.PropTypes.bool,
    canMigrateResources: React.PropTypes.bool,
  };

  onStartUpdateDestination = () => {
    return this.onStartUpdate('Destination');
  };

  onStartUpdateSource = () => {
    return this.onStartUpdate('Source');
  };

  onStartMigration = (templateCode, migratorTypeName, resourceCode, target) => {
    console.log('migration', templateCode, migratorTypeName, resourceCode, target);

  };

  onStartMigration = (templateCode, migratorTypeName, resourceCode, target) => {
    if (!this.state.isProcessing){

      this.setState({
        isProcessing: true,
        processSuccessful: false,
      });

      console.log('updating resources');

      Relay.Store.commitUpdate(
        new UpdateResourcesMutation({
          templateCode: templateCode,
          migratorTypeName: migratorTypeName,
          resourceCode: resourceCode,
          target: target
        }),
        {
          onSuccess: (response) => {
            console.log('response', response);
            const responsePayload = response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationResourceUpdate;

            if (responsePayload.errors &&
              responsePayload.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(responsePayload.errors.edges);
              this.props.onError(messages);

              this.setState({
                processSuccessful: false,
                processErrors: messages,
              });
            } else {
              console.log('result update nodes', responsePayload.result);
              // total success
              this.setState({
                isProcessing: false,
                processErrors: null,
                processSuccessful: true,
              });

              this.props.onStateChange();
            }
          },
          onFailure: (transaction) => {
            this.setState({
              isProcessing: false
            });
            console.log(transaction)},
        },
      );
    }
  };

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  };

  render() {
    let processClassName = '';
    if (this.state.isProcessing) {
      processClassName += ' fa-spin';
    }

    return (
      <div>
        <h3>Resources list</h3>
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Name</th>
              <th>Code</th>
              <th>Position</th>
              <th>Action</th>
            </tr>
          </thead>
          {this.props.migrationState && this.props.migrationState.templateStates.edges && this.props.migrationState.templateStates.edges.map((edge) => {
            const node = edge.node;

            return (
              <tbody key={`${node.code}`}>
                <tr>
                  <th colSpan={4}>{node.code}</th>
                </tr>
                {node.migrators.edges.map((migratorEdge) => {
                  const migratorNode = migratorEdge.node;
                  const resources = migratorNode.resources.edges;

                  return resources.map((resourceEdge) => {
                    const resourceNode = resourceEdge.node;
                    const direction = resourceNode.position === 'Source' ? 'Destination' : 'Source';
                    const iconName = direction === 'Source' ? 'backward' : 'forward';
                    const directionName = direction === 'Source' ? 'Downgrade resources' : 'Upgrade resources';

                    return (
                      <tr key={resourceNode.code}>
                        <td className="migration-resources">{resourceNode.name}</td>
                        <td className="migration-resources">{resourceNode.code}</td>
                        <td className="migration-resources">{resourceNode.position}</td>
                        <td>
                          {this.props.canMigrateResources &&
                            <button className="btn btn-primary" type="button" onClick={() => {this.onStartMigration(node.code, migratorNode.typeName, resourceNode.code, direction)}}>
                              <Icon name={iconName} className={processClassName}/>{' '}{directionName}
                            </button>
                          }
                        </td>
                      </tr>
                    );
                  });

                })}
              </tbody>
            )
          })
          }
        </table>
      </div>
    );
  }
}

export default Relay.createContainer(
  UpdateResources,
  {
    fragments: {
      migrationState: () => Relay.QL`fragment on IClusterKitNodeApi_MigrationActorMigrationState {
        templateStates {
          edges {
            node {
              code
              position
              migrators {
                edges {
                  node {
                    typeName
                    name
                    position
                    direction
                    resources {
                      edges {
                        node {
                          sourcePoint
                          destinationPoint
                          position
                          migrationToSourceExecutor
                          migrationToDestinationExecutor
                          name
                          code
                          currentPoint
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

