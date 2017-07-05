import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import FinishMigrationMutation from './mutations/FinishMigrationMutation';

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
    canFinishMigration: React.PropTypes.bool,
    onStateChange: React.PropTypes.func,
    operationIsInProgress: React.PropTypes.bool,
  };

  onFinishMigration = (target) => {
    if (!this.state.isProcessing){

      this.setState({
        isProcessing: true,
        processSuccessful: false,
      });

      console.log('finishing migration');

      Relay.Store.commitUpdate(
        new FinishMigrationMutation({}),
        {
          onSuccess: (response) => {
            console.log('response', response);
            const responsePayload = response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationFinish;

            if (responsePayload.errors &&
              responsePayload.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(responsePayload.errors.edges);

              this.setState({
                processSuccessful: false,
                processErrors: messages
              });
            } else {
              console.log('result finish migration', responsePayload.result);
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

      {this.props.canFinishMigration && !this.state.isProcessing && !this.props.operationIsInProgress &&
        <button className="btn btn-success" type="button" onClick={this.onFinishMigration}>
          <Icon name="flag-checkered" className={processClassName}/>{' '}Finish migration
        </button>
      }
    </div>
    );
  }
}

export default FinishMigration
