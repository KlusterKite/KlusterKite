import React from 'react'
import Relay from 'react-relay'
// import { browserHistory } from 'react-router'

import delay from 'lodash/delay'
// import isEqual from 'lodash/isEqual'

import MigrationSteps from '../../components/MigrationOperations/MigrationSteps'

import DateFormat from '../../utils/date';

class MigrationPage extends React.Component {

  static propTypes = {
    api: React.PropTypes.object,
    params: React.PropTypes.object,
  };

  static contextTypes = {
    router: React.PropTypes.object,
  };

  constructor (props) {
    super(props);
    this.state = {
      operationIsInProgress: false,
      migrationHasFinished: false,
      processErrors: null,
    };

    this.onStateChange = this.onStateChange.bind(this);
  }

  componentDidMount = () => {
    delay(() => this.refetchDataOnTimer(), 5000);
  };

  componentWillUnmount = () => {
    clearTimeout(this._refreshId);
  };

  componentWillMount() {
    this.onReceiveProps(this.props);
  }

  componentWillReceiveProps(nextProps) {
    this.onReceiveProps(nextProps);
  }

  onReceiveProps(nextProps) {
    console.log('nextProps', nextProps);

    if (this.props.api && this.props.api.clusterKitNodesApi.clusterManagement.currentMigration && !nextProps.api.clusterKitNodesApi.clusterManagement.currentMigration) {
      // Migration just finshed
      console.log('Migration has been finished!');

      this.setState({
        migrationHasFinished: true,
      })
    }

    if (nextProps.api) {
      console.log('operationIsInProgress: ', nextProps.api.clusterKitNodesApi.clusterManagement.resourceState.operationIsInProgress);
      this.setState({
        operationIsInProgress: nextProps.api.clusterKitNodesApi.clusterManagement.resourceState.operationIsInProgress
      });
    }
  }

  refetchDataOnTimer = () => {
    this.props.relay.forceFetch();
    this._refreshId = delay(() => this.refetchDataOnTimer(), 5000);
  };

  onStateChange() {
    console.log('onStateChange.');

    this.setState({
      operationIsInProgress: true,
      processErrors: null,
    });
  }

  onError(errors) {
    console.log('onError', errors);

    this.setState({
      processErrors: errors,
    });
  }

  render () {
    const clusterManagement = this.props.api.clusterKitNodesApi.clusterManagement;
    const currentMigration = clusterManagement.currentMigration;
    const resourceState = clusterManagement.resourceState;
    const nodesUpdating = resourceState.currentMigrationStep === 'NodesUpdating';

    return (
      <div>
        {currentMigration &&
          <div>
            <h2>Migration {currentMigration.fromRelease.name} → {currentMigration.toRelease.name}</h2>
            <p>Created: {currentMigration.started && DateFormat.formatDateTime(new Date(currentMigration.started))}</p>

            {(nodesUpdating || this.state.operationIsInProgress) && !this.state.migrationHasFinished &&
            <div className="alert alert-warning" role="alert">
              <span className="glyphicon glyphicon-time fa-spin" aria-hidden="true"></span>
              {' '}
              Operation in progress, please wait…
            </div>
            }

            {this.state.migrationHasFinished &&
            <div className="alert alert-success" role="alert">
              <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
              {' '}
              Migration finished successfully!
            </div>
            }

            {this.state.processErrors && this.state.processErrors.map((error, index) => {
              return (
                <div className="alert alert-danger" role="alert" key={`error-${index}`}>
                  <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
                  {' '}
                  {error}
                </div>
              );
            })
            }

            {!nodesUpdating && !this.state.operationIsInProgress && !this.state.migrationHasFinished && !resourceState.canUpdateNodesToDestination && !resourceState.canFinishMigration &&
            <div className="alert alert-warning" role="alert">
              <span className="glyphicon glyphicon-time" aria-hidden="true"></span>
              {' '}
              All operations completed, but migration cannot be finished right now. Please wait.
            </div>
            }

            {!nodesUpdating && !this.state.operationIsInProgress && !this.state.migrationHasFinished && resourceState.canUpdateNodesToDestination && !resourceState.canMigrateResources && !resourceState.canCancelMigration &&
            <div className="alert alert-warning" role="alert">
              <span className="glyphicon glyphicon-time" aria-hidden="true"></span>
              {' '}
              Migration seems to be in init state, but cannot be cancelled right now.
            </div>
            }
          </div>
        }

        {!currentMigration &&
          <p>No active migration found.</p>
        }

        <MigrationSteps
          migrationSteps={resourceState.migrationSteps}
          currentMigrationStep={resourceState.currentMigrationStep}
          onStateChange={this.onStateChange}
          onError={this.onError}
          canUpdateForward={resourceState.canUpdateNodesToDestination}
          canUpdateBackward={resourceState.canUpdateNodesToSource}
          canCancelMigration={resourceState.canCancelMigration}
          canFinishMigration={resourceState.canFinishMigration}
          operationIsInProgress={this.state.operationIsInProgress}
        />
      </div>
    )
  }
}

export default Relay.createContainer(
  MigrationPage,
  {
    fragments: {
      api: () => Relay.QL`
        fragment on IClusterKitNodeApi {
          clusterKitNodesApi {
            clusterManagement {
              currentMigration {
                state
                started
                fromRelease {
                  name
                }
                toRelease {
                  name
                }
              }
              resourceState {
                operationIsInProgress
                canUpdateNodesToDestination
                canUpdateNodesToSource
                canCancelMigration
                canFinishMigration
                canMigrateResources
                migrationSteps
                currentMigrationStep
              }
            }
          }
        }
      `,
    },
  },
)
