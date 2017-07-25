import React from 'react';
import Relay from 'react-relay'

import MigrationInProgressWarning from './MigrationInProgressWarning'
// import NotInSourcePositionWarning from './NotInSourcePositionWarning'
// import OperationIsInProgressWarning from './OperationIsInProgressWarning'
// import OutOfScopeWarning from './OutOfScopeWarning'

export class Warnings extends React.Component {
  static propTypes = {
    migrationWarning: React.PropTypes.bool,
    operationIsInProgressWarning: React.PropTypes.bool,
    migrationBrokenWarning: React.PropTypes.bool,
    notInSourcePositionWarning: React.PropTypes.bool,
  };

  render() {
    const klusterKiteNodesApi = this.props.klusterKiteNodesApi;

    return (
      <div>
        {this.props.migrationWarning &&
          <MigrationInProgressWarning clusterManagement={klusterKiteNodesApi.clusterManagement} />
        }
      </div>
    );
  }
}

/*
        {this.props.operationIsInProgressWarning &&
          <OperationIsInProgressWarning resourceState={klusterKiteNodesApi.clusterManagement.resourceState} />
        }
        {this.props.notInSourcePositionWarning &&
          <NotInSourcePositionWarning resourceState={klusterKiteNodesApi.clusterManagement.resourceState}/>
        }
        {this.props.migrationBrokenWarning &&
          <OutOfScopeWarning resourceState={klusterKiteNodesApi.clusterManagement.resourceState}/>
        }
 */

export default Relay.createContainer(
  Warnings,
  {
    fragments: {
      klusterKiteNodesApi: () => Relay.QL`fragment on IKlusterKiteNodeApi_Root {
        clusterManagement {
          ${MigrationInProgressWarning.getFragment('clusterManagement')}
        }
      }
      `,
    },
  },
)

/*
          resourceState {
            ${OperationIsInProgressWarning.getFragment('resourceState')},
            ${OutOfScopeWarning.getFragment('resourceState')},
            ${NotInSourcePositionWarning.getFragment('resourceState')},

 */