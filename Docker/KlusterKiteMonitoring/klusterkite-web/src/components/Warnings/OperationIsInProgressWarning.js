import React from 'react';
import Relay from 'react-relay'

export class OperationIsInProgressWarning extends React.Component {
  render() {
    return (
      <div>
        {this.props.resourceState && this.props.resourceState.operationIsInProgress &&
          <div className="alert alert-warning" role="alert">
            <span className="glyphicon glyphicon-alert" aria-hidden="true"></span>
            {' '}
            Operation is in progress. Keep calm and wait.
          </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  OperationIsInProgressWarning,
  {
    fragments: {
      resourceState: () => Relay.QL`fragment on IKlusterKiteNodeApi_ResourceState {
        operationIsInProgress
      }
      `,
    },
  },
)
