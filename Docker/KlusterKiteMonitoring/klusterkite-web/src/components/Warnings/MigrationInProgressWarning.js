import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

export class MigrationInProgressWarning extends React.Component {
  render() {
    const clusterManagement = this.props.clusterManagement;

    return (
      <div>
        {clusterManagement.currentMigration &&
          <div className="alert alert-warning" role="alert">
            <span className="glyphicon glyphicon-alert" aria-hidden="true"></span>
            {' '}
            Migration is in progress! Please <Link to={'/klusterkite/Migration/'}>finish it</Link>.
          </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  MigrationInProgressWarning,
  {
    fragments: {
      clusterManagement: () => Relay.QL`fragment on IKlusterKiteNodeApi_ClusterManagement {
        currentMigration {
          state
        }
      }
      `,
    },
  },
)
