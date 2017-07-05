import React from 'react'
import Relay from 'react-relay'
// import { browserHistory } from 'react-router'

import delay from 'lodash/delay'
// import isEqual from 'lodash/isEqual'

// import CreateReleaseMutation from './mutations/CreateReleaseMutation'

import CancelMigration from '../../components/MigrationOperations/CancelMigration'
import FinishMigration from '../../components/MigrationOperations/FinishMigration'
import UpdateNodes from '../../components/MigrationOperations/UpdateNodes'

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
    });
  }

  render () {
    const clusterManagement = this.props.api.clusterKitNodesApi.clusterManagement;
    const currentMigration = clusterManagement.currentMigration;
    const resourceState = clusterManagement.resourceState;

    return (
      <div>
        {currentMigration &&
          <div>
            <h2>Migration {currentMigration.started && DateFormat.formatDateTime(new Date(currentMigration.started))}</h2>

            {this.state.operationIsInProgress && !this.state.migrationHasFinished &&
            <div className="alert alert-warning" role="alert">
              <span className="glyphicon glyphicon-time" aria-hidden="true"></span>
              {' '}
              Operation is in progress, please wait…
            </div>
            }

            {this.state.migrationHasFinished &&
            <div className="alert alert-success" role="alert">
              <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
              {' '}
              Migration finished successfully!
            </div>
            }

            {!this.state.operationIsInProgress && !this.state.migrationHasFinished && !resourceState.canUpdateNodesToDestination && !resourceState.canFinishMigration &&
            <div className="alert alert-warning" role="alert">
              <span className="glyphicon glyphicon-time" aria-hidden="true"></span>
              {' '}
              All operations completed, but migration cannot be finished right now. Please wait.
            </div>
            }

            {!this.state.operationIsInProgress && !this.state.migrationHasFinished && resourceState.canUpdateNodesToDestination && !resourceState.canMigrateResources && !resourceState.canCancelMigration &&
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

        {currentMigration &&
          <div>
            <table className="table">
              <thead>
              <tr>
                <th>#</th>
                <th>Name</th>
                <th>Type</th>
                <th>State</th>
                <th>Action</th>
              </tr>
              </thead>
              <tbody>
              {resourceState.canMigrateResources &&
              <tr className="success">
                <th scope="row">1</th>
                <td>Database</td>
                <td>Resource</td>
                <td>Updated</td>
                <td></td>
              </tr>
              }
              <tr>
                <th scope="row">1</th>
                <td>Nodes</td>
                <td>Nodes</td>
                <td>
                  {resourceState.canUpdateNodesToDestination &&
                  <span className="label label-warning">Needs update</span>
                  }
                  {resourceState.canUpdateNodesToSource &&
                  <span className="label label-success">Updated</span>
                  }
                  {!resourceState.canUpdateNodesToDestination && !resourceState.canUpdateNodesToSource &&
                  <span className="label label-default">Not available</span>
                  }
                </td>
                <td>
                  <UpdateNodes
                    onStateChange={this.onStateChange}
                    canUpdateForward={resourceState.canUpdateNodesToDestination}
                    canUpdateBackward={resourceState.canUpdateNodesToSource}
                    operationIsInProgress={this.state.operationIsInProgress}
                  />
                </td>
              </tr>
              </tbody>
            </table>
          </div>
        }

        <CancelMigration
          canCancelMigration={resourceState.canCancelMigration}
          onStateChange={this.onStateChange}
          operationIsInProgress={this.state.operationIsInProgress}
        />

        <FinishMigration
          canFinishMigration={resourceState.canFinishMigration}
          onStateChange={this.onStateChange}
          operationIsInProgress={this.state.operationIsInProgress}
        />
      </div>
    )
  }
}

/*<div className="panel panel-default">
 <div className="panel-body">
 Panel content
 </div>
 </div>*/

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
              }
              resourceState {
                operationIsInProgress
                canUpdateNodesToDestination
                canUpdateNodesToSource
                canCancelMigration
                canFinishMigration
                canMigrateResources
              }
            }
          }
        }
      `,
    },
  },
)
