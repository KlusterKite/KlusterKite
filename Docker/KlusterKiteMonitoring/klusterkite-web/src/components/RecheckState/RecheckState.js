import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import RecheckStateMutation from './mutations/RecheckStateMutation'

export class RecheckState extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isProcessing: false,
    };
  }

  onStartRecheck() {
    if (!this.state.isProcessing){

      this.setState({
        isProcessing: true,
        processSuccessful: false,
      });

      Relay.Store.commitUpdate(
        new RecheckStateMutation(),
        {
          onSuccess: (response) => {
            console.log('response', response);
            const responsePayload = response.klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_recheckState;

            if (responsePayload.errors &&
              responsePayload.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(responsePayload.errors.edges);
              this.props.onError(messages);

              this.setState({
                processSuccessful: false,
                processErrors: messages,
              });
            } else {
              // console.log('result update nodes', responsePayload.result);
              // total success
              this.setState({
                isProcessing: false,
                processErrors: null,
                processSuccessful: true,
              });
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
  }

  render() {
    const processClassName = this.state.isProcessing ? 'fa-spin' : '';

    return (
      <button className="btn btn-info" type="button" onClick={this.onStartRecheck.bind(this)} disabled={this.state.isProcessing}>
        <Icon name="refresh" className={processClassName}/>{' '}Recheck state
      </button>
    );
  }
}

export default RecheckState
