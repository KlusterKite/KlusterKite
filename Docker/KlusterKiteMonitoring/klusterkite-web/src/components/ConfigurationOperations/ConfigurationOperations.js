import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';
import { Link } from 'react-router';

// import delay from 'lodash/delay'

import DraftOperations from './DraftOperations';
import ReadyOperations from './ReadyOperations';
import ActiveOperations from './ActiveOperations';
import ArchivedOperations from './ArchivedOperations';
import ObsoleteOperations from './ObsoleteOperations';

import './styles.css';

export class ConfigurationOperations extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isChecking: false,
      checkSuccess: false,
      checkCompatibleTemplates: null,
      checkActiveNodes: null,
      isSettingReady: false,
      isUpdating: false,
      isChangingState: false
    };
  }

  static propTypes = {
    configuration: React.PropTypes.object,
    nodeManagement: React.PropTypes.object,
    isStable: React.PropTypes.bool.isRequired,
    configurationId: React.PropTypes.string.isRequired,
    configurationInnerId: React.PropTypes.number.isRequired,
    currentState: React.PropTypes.string.isRequired,
    onForceFetch: React.PropTypes.func.isRequired,
    onStartMigration: React.PropTypes.func.isRequired,
  };

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  };

  render() {
    const nodeTemplates = this.props.configuration && this.props.configuration.nodeTemplates && this.props.configuration.nodeTemplates.edges;

    return (
      <div>
        <h2>Operations</h2>
        <div>
          <DraftOperations
            nodeTemplates={nodeTemplates}
            configurationId={this.props.configurationId}
            configurationInnerId={this.props.configurationInnerId}
            currentState={this.props.currentState}
            onForceFetch={this.props.onForceFetch}
          />

          <ReadyOperations
            configurationId={this.props.configurationId}
            configurationInnerId={this.props.configurationInnerId}
            currentState={this.props.currentState}
            onForceFetch={this.props.onForceFetch}
            canCreateMigration={this.props.nodeManagement.resourceState.canCreateMigration}
            currentMigration={this.props.nodeManagement.currentMigration}
            onStartMigration={this.props.onStartMigration}
          />

          <ActiveOperations
            configurationId={this.props.configurationId}
            configurationInnerId={this.props.configurationInnerId}
            currentState={this.props.currentState}
            onForceFetch={this.props.onForceFetch}
            isStable={this.props.isStable}
          />

          <ArchivedOperations
            configurationId={this.props.configurationId}
            configurationInnerId={this.props.configurationInnerId}
            currentState={this.props.currentState}
            onForceFetch={this.props.onForceFetch}
            canCreateMigration={this.props.nodeManagement.resourceState.canCreateMigration}
            currentMigration={this.props.nodeManagement.currentMigration}
            onStartMigration={this.props.onStartMigration}
          />

          <ObsoleteOperations
            currentState={this.props.currentState}
          />
        </div>
        <h2 className="margined-header">Settings</h2>
        {this.props.currentState && this.props.currentState === 'Draft' && !this.state.isChangingState &&
          <div className="buttons-block-margin">
            <Link to={`/klusterkite/CopyConfig/${this.props.configurationId}/updateCurrent`} className="btn btn-success" role="button">
              <Icon name="clone"/>{' '}Update packages
            </Link>

            {false && <Link to={`/klusterkite/CopyConfig/${this.props.configurationId}/exact`} className="btn btn-success btn-margined"
                  role="button">
              <Icon name="clone"/>{' '}Clone configuration (exact)
            </Link>}
          </div>
        }

        {this.state.isChangingState &&
        <div className="alert alert-warning" role="alert">
          <span className="glyphicon glyphicon-time" aria-hidden="true"></span>
          {' '}
          Please wait, expecting server reply…
        </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  ConfigurationOperations,
  {
    fragments: {
      configuration: () => Relay.QL`fragment on IKlusterKiteNodeApi_ConfigurationSettings {
        nodeTemplates {
          edges {
            node {
              code
            }
          }
        }
      }`,
      nodeManagement: () => Relay.QL`fragment on IKlusterKiteNodeApi_ClusterManagement {
        resourceState {
          canCreateMigration
        }
        currentMigration {
          state
        }
      }`,
    },
  },
)
