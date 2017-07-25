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
      selectedResources: [],
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

  onStartMassMigration = () => {
    if (!this.state.isProcessing){
      this.setState({
        isProcessing: true,
        processSuccessful: false,
      });

      Relay.Store.commitUpdate(
        new UpdateResourcesMutation({
          resources: this.prepareResourceListForMigration(this.state.selectedResources)
        }),
        {
          onSuccess: (response) => {
            const responsePayload = response.klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationResourceUpdate;

            if (responsePayload.errors &&
              responsePayload.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(responsePayload.errors.edges);
              this.props.onError(messages);

              this.setState({
                processSuccessful: false,
                processErrors: messages,
              });
            } else {
              // console.log('result update nodes', responsePayload.result);
              // total success
              this.setState({
                isProcessing: false,
                processErrors: null,
                processSuccessful: true,
                selectedResources: [],
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

  /**
   * Prepare resource list for migration by removal unnecessary keys
   * @param resources {Object[]} Resources List
   * @return {Object[]} Prepared resources list
   */
  prepareResourceListForMigration = (resources) => {
    let resourceList = [];

    resources.forEach((item) => {
      resourceList.push({
        templateCode: item.templateCode,
        migratorTypeName: item.migratorTypeName,
        resourceCode: item.resourceCode,
        target: item.target,
      })
    });

    return resourceList;
  };

  onSelectResource = (checked, key, templateCode, migratorTypeName, resourceCode, target) => {
    const resource = {
      templateCode: templateCode,
      migratorTypeName: migratorTypeName,
      resourceCode: resourceCode,
      target: target,
      key: key,
    };

    if (checked) {
      this.setState((prevState) => ({
        selectedResources: [
          ...prevState.selectedResources,
          resource,
        ],
      }));
    } else {
      this.setState((prevState) => ({
        selectedResources: prevState.selectedResources.filter(item => item.key !== resource.key),
      }));
    }

  };

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  };

  render() {
    const upgradePossible = this.state.selectedResources.some(item => item.target === 'Destination');
    const downgradePossible = this.state.selectedResources.some(item => item.target === 'Source');
    const isProcessing = this.props.operationIsInProgress || this.state.isProcessing;

    return (
      <div>
        <h3>Resources list</h3>
        {isProcessing &&
        <div className="alert alert-warning" role="alert">
          <span className="glyphicon glyphicon-time fa-spin" aria-hidden="true"></span>
          {' '}
          Operation in progress, please wait…
        </div>
        }

        <button className="btn btn-primary" type="button" onClick={() => {this.onStartMassMigration()}} disabled={isProcessing || (!upgradePossible && !downgradePossible)}>
          <Icon name='forward' />{' '}
          {upgradePossible && !downgradePossible && <span>Upgrade</span>}
          {downgradePossible && !upgradePossible && <span>Downgrade</span>}
          {!upgradePossible && !downgradePossible && <span>Process</span>}
          {downgradePossible && upgradePossible && <span>Upgrade and downgrade</span>}
          {' '}selected{' '}
          {this.state.selectedResources.length === 1 && <span>resouce</span>}
          {this.state.selectedResources.length !== 1 && <span>resouces</span>}
        </button>

        {this.props.migrationState && this.props.migrationState.templateStates.edges && this.props.migrationState.templateStates.edges.map((edge) => {
          const node = edge.node;
          const migratableResources = this.props.migrationState.migratableResources.edges.map(edge => edge.node);

          return (
            <div key={node.code}>
              <h4 className="migration-title">{node.code}</h4>
              <table className="table table-hover">
                <thead>
                <tr>
                  <th>Name</th>
                  <th>Code</th>
                  <th>Position</th>
                  <th>Current point</th>
                  <th className="migration-upgrade" title="Upgrade selected resources">↑</th>
                  <th className="migration-downgrade" title="Downgrade selected resources">↓</th>
                </tr>
                </thead>
                {node.migrators.edges.map((migratorEdge) => {
                  const migratorNode = migratorEdge.node;
                  const resources = migratorNode.resources.edges;

                  return (
                    <tbody key={migratorNode.typeName}>
                      <tr>
                        <th colSpan={5}>{migratorNode.name}</th>
                      </tr>
                      {resources.map((resourceEdge) => {
                        const resourceNode = resourceEdge.node;
                        const direction = (resourceNode.position === 'Source' || resourceNode.position === 'NotCreated') ? 'Destination' : 'Source';
                        const isMigratable = migratableResources.some(x => x.key === resourceNode.key);

                        return (
                          <tr key={resourceNode.code}>
                            <td className="migration-resources">{resourceNode.name}</td>
                            <td className="migration-resources">{resourceNode.code}</td>
                            <td className="migration-resources">{resourceNode.position}</td>
                            <td className="migration-resources">{resourceNode.currentPoint}</td>
                            <td className="migration-resources migration-upgrade">
                              {isMigratable && resourceNode.migrationToDestinationExecutor !== null &&
                                <input
                                  type="checkbox"
                                  checked={this.state.selectedResources.some(item => item.target === direction && item.key === resourceNode.key)}
                                  onChange={(element) => this.onSelectResource(element.target.checked, resourceNode.key, node.code, migratorNode.typeName, resourceNode.code, direction)}
                                  disabled={isProcessing}
                                />
                              }
                            </td>
                            <td className="migration-resources migration-downgrade">
                              {isMigratable && resourceNode.migrationToSourceExecutor !== null &&
                                <input
                                  type="checkbox"
                                  checked={this.state.selectedResources.some(item => item.target === direction && item.key === resourceNode.key)}
                                  onChange={(element) => this.onSelectResource(element.target.checked, resourceNode.key, node.code, migratorNode.typeName, resourceNode.code, direction)}
                                  disabled={isProcessing}
                                />
                              }
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  )
                })}
              </table>
            </div>
          )
        })}
      </div>
    );
  }
}

export default Relay.createContainer(
  UpdateResources,
  {
    fragments: {
      migrationState: () => Relay.QL`fragment on IKlusterKiteNodeApi_MigrationActorMigrationState {
        templateStates   {
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
                          key
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
        migratableResources {
          edges {
            node {
              key,
            }
          }
        }
      }
      `,
    },
  },
)

