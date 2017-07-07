import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import CancelMigrationMutation from './mutations/FinishMigrationMutation';

import './styles.css';

export class FinishMigration extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isProcessing: false,
      processSuccessful: false,
      processErrors: null,
    };
  }

  static propTypes = {
    canCancelMigration: React.PropTypes.bool,
    onStateChange: React.PropTypes.func.isRequired,
    onError: React.PropTypes.func.isRequired,
    operationIsInProgress: React.PropTypes.bool,
  };

  onFinishMigration = (target) => {
    if (!this.state.isProcessing){

      this.setState({
        isProcessing: true,
        processSuccessful: false,
      });

      console.log('cancelling migration');

      Relay.Store.commitUpdate(
        new CancelMigrationMutation({}),
        {
          onSuccess: (response) => {
            console.log('response', response);
            const responsePayload = response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationCancel;

            if (responsePayload.errors &&
              responsePayload.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(responsePayload.errors.edges);
              this.props.onError(messages);

              this.setState({
                processSuccessful: false,
                processErrors: messages
              });
            } else {
              console.log('result cancel migration', responsePayload.result);
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
            console.log(transaction);
          },
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
      <div className="buttons-block-margin">
        {this.props.canCancelMigration && !this.state.isProcessing && !this.props.operationIsInProgress &&
          <button className="btn btn-danger" type="button" onClick={this.onFinishMigration}>
            <Icon name="trash" className={processClassName}/>{' '}Cancel migration
          </button>
        }
      </div>
    );
  }
}

export default FinishMigration
