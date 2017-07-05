import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import UpdateNodesMutation from './mutations/UpdateNodesMutation';

export class UpdateNodes extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isProcessing: false,
      processSuccessful: false,
      processErrors: null,
    };
  }

  static propTypes = {
    canUpdateForward: React.PropTypes.bool,
    canUpdateBackward: React.PropTypes.bool,
    onStateChange: React.PropTypes.func,
    operationIsInProgress: React.PropTypes.bool,
  };

  onStartUpdateDestination = () => {
    return this.onStartUpdate('Destination');
  }

  onStartUpdateSource = () => {
    return this.onStartUpdate('Source');
  }

  onStartUpdate = (target) => {
    if (!this.state.isProcessing){

      this.setState({
        isProcessing: true,
        processSuccessful: false,
      });

      console.log('updating nodes');

      Relay.Store.commitUpdate(
        new UpdateNodesMutation({
          target: target,
        }),
        {
          onSuccess: (response) => {
            console.log('response', response);
            const responsePayload = response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationNodesUpdate;

            if (responsePayload.errors &&
              responsePayload.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(responsePayload.errors.edges);

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

      {this.props.canUpdateForward && !this.state.isProcessing && !this.props.operationIsInProgress &&
        <button className="btn btn-primary" type="button" onClick={this.onStartUpdateDestination}>
          <Icon name="forward" className={processClassName}/>{' '}Update nodes
        </button>
      }

      {this.props.canUpdateBackward && !this.state.isProcessing && !this.props.operationIsInProgress &&
      <button className="btn btn-primary" type="button" onClick={this.onStartUpdateSource}>
        <Icon name="backward" className={processClassName}/>{' '}Rollback nodes
      </button>
      }
    </div>
    );
  }
}

export default UpdateNodes
