import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import delay from 'lodash/delay'

import CheckReleaseMutation from './mutations/CheckReleaseMutation';
import SetReadyMutation from './mutations/SetReadyMutation';

import './styles.css';

export class DraftOperations extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isChecking: false,
      checkSuccess: false,
      checkCompatibleTemplates: null,
      checkActiveNodes: null,
      isChangingState: false
    };
  }

  static propTypes = {
    nodeTemplates: React.PropTypes.arrayOf(React.PropTypes.object),
    releaseId: React.PropTypes.string.isRequired,
    releaseInnerId: React.PropTypes.number.isRequired,
    currentState: React.PropTypes.string.isRequired,
    onForceFetch: React.PropTypes.func.isRequired,
  };

  onCheck = () => {
    if (!this.state.isChecking) {
      console.log('checking release');

      this.setState({
        isChecking: true,
        checkSuccess: false,
      });

      Relay.Store.commitUpdate(
        new CheckReleaseMutation(
          {
            releaseId: this.props.releaseInnerId,
          }),
        {
          onSuccess: (response) => {
            console.log('response', response);
            if (response.klusterKiteNodeApi_klusterKiteNodesApi_releases_check.errors &&
              response.klusterKiteNodeApi_klusterKiteNodesApi_releases_check.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_releases_check.errors.edges);

              this.setState({
                isChecking: false,
                checkErrors: messages
              });
            } else {
              console.log('result check', response.klusterKiteNodeApi_klusterKiteNodesApi_releases_check);
              // total success
              this.setState({
                isChecking: false,
                checkErrors: null,
                checkSuccess: true,
                checkCompatibleTemplates: response.klusterKiteNodeApi_klusterKiteNodesApi_releases_check.node.compatibleTemplatesBackward.edges,
                checkActiveNodes: response.klusterKiteNodeApi_klusterKiteNodesApi_releases_check.api.klusterKiteNodesApi.getActiveNodeDescriptions.edges,
              });
            }
          },
          onFailure: (transaction) => {
            this.setState({
              isChecking: false
            });
            console.log(transaction)},
        },
      )
    }
  };

  onSetReady = () => {
    if (!this.state.isSettingReady){
      console.log('set ready');

      this.setState({
        isSettingReady: true,
        setReadySuccessful: false,
        isChangingState: true
      });

      Relay.Store.commitUpdate(
        new SetReadyMutation(
          {
            releaseId: this.props.releaseInnerId,
          }),
        {
          onSuccess: (response) => {
            console.log('response', response);
            if (response.klusterKiteNodeApi_klusterKiteNodesApi_releases_setReady.errors &&
              response.klusterKiteNodeApi_klusterKiteNodesApi_releases_setReady.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_releases_setReady.errors.edges);

              this.setState({
                isSettingReady: false,
                setReadyErrors: messages,
                isChangingState: false
              });
            } else {
              console.log('result set ready', response.klusterKiteNodeApi_klusterKiteNodesApi_releases_setReady);
              // total success
              this.setState({
                isSettingReady: false,
                setReadyErrors: null,
                setReadySuccessful: true,
              });

              this.refetchAfterDelay(5000);
            }
          },
          onFailure: (transaction) => {
            this.setState({
              isSettingReady: false,
              isChangingState: false
            });
            console.log(transaction)},
        },
      );
    }
  };

  /**
   * Refetches data from GraphQL server after a delay
   * @param delayTime {number} Delay in ms
   */
  refetchAfterDelay = (delayTime) => {
    delay(() => {
      this.props.onForceFetch();
    }, delayTime);

    delay(() => {
      this.setState({
        isChangingState: false
      });
    }, delayTime + 1000);
  };

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  };

  render() {
    let checkClassName = '';
    if (this.state.isChecking) {
      checkClassName += ' fa-spin';
    }

    let setReadyClassName = '';
    if (this.state.isSettingReady) {
      setReadyClassName += ' fa-spin';
    }

    return (
    <div>
      {this.state.checkErrors && this.state.checkErrors.map((error, index) => {
        return (
          <div className="alert alert-danger" role="alert" key={`error-${index}`}>
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            {' '}
            {error}
          </div>
        );
      })
      }

      {this.state.setReadyErrors && this.state.setReadyErrors.map((error, index) => {
        return (
          <div className="alert alert-danger" role="alert" key={`error-${index}`}>
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            {' '}
            {error}
          </div>
        );
      })
      }

      {this.state.checkSuccess &&
      <div>
        <div className="alert alert-success" role="alert">
          <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
          {' '}
          Check successful!
        </div>
      </div>
      }

      {this.state.setReadySuccessful && !this.state.isChangingState && 
      <div>
        <div className="alert alert-success" role="alert">
          <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
          {' '}
          Set ready successful!
        </div>
      </div>
      }

      {this.props.currentState && this.props.currentState === 'Draft' && !this.state.isChangingState &&
      <button className="btn btn-default" type="button" onClick={this.onCheck}>
        <Icon name="check-circle" className={checkClassName}/>{' '}Check release
      </button>
      }

      {this.props.currentState && this.props.currentState === 'Draft' && !this.state.isChangingState &&
      <button className="btn btn-default btn-margined" type="button" onClick={this.onSetReady}>
        <Icon name="flag-checkered" className={setReadyClassName}/>{' '}Set ready
      </button>
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

export default DraftOperations
