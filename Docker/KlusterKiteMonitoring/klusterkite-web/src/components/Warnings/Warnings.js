import React from 'react';
import Relay from 'react-relay'

import MigrationInProgressWarning from './MigrationInProgressWarning'
import OperationIsInProgressWarning from './OperationIsInProgressWarning'
import MigratableResourcesWarning from './MigratableResourcesWarning'
import OutOfScopeWarning from './OutOfScopeWarning'

export class Warnings extends React.Component {
  static propTypes = {
    migrationWarning: React.PropTypes.bool,
    operationIsInProgressWarning: React.PropTypes.bool,
    migratableResourcesWarning: React.PropTypes.bool,
    outOfScopeWarning: React.PropTypes.bool,
  };

  render() {
    const klusterKiteNodesApi = this.props.klusterKiteNodesApi;

    return (
      <div>
        {this.props.migrationWarning &&
          <MigrationInProgressWarning clusterManagement={klusterKiteNodesApi.clusterManagement} />
        }
        {this.props.operationIsInProgressWarning &&
          <OperationIsInProgressWarning resourceState={klusterKiteNodesApi.clusterManagement.resourceState} />
        }
        {this.props.migratableResourcesWarning &&
          <MigratableResourcesWarning resourceState={klusterKiteNodesApi.clusterManagement.resourceState} />
        }
        {this.props.outOfScopeWarning &&
          <OutOfScopeWarning resourceState={klusterKiteNodesApi.clusterManagement.resourceState} />
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  Warnings,
  {
    fragments: {
      klusterKiteNodesApi: () => Relay.QL`fragment on IKlusterKiteNodeApi_Root {
        clusterManagement {
          ${MigrationInProgressWarning.getFragment('clusterManagement')}
          resourceState {
            ${OperationIsInProgressWarning.getFragment('resourceState')},
            ${MigratableResourcesWarning.getFragment('resourceState')},
            ${OutOfScopeWarning.getFragment('resourceState')},
          }
        }
      }
      `,
    },
  },
)