import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';
import delay from 'lodash/delay'

import Modal from '../Form/Modal'

import RollbackClusterMutation from './mutations/RollbackClusterMutation';

import './styles.css';

export default class ObsoleteOperations extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isRollbacking: false,
      rollbackSuccessful: false,
      isChangingState: false,
      showConfirmationRollback: false,
    };
  }

  static propTypes = {
    releaseId: React.PropTypes.string.isRequired,
    releaseInnerId: React.PropTypes.number.isRequired,
    currentState: React.PropTypes.string.isRequired,
    onForceFetch: React.PropTypes.func.isRequired,
  };

  onRollbackRequestConfirmation = () => {
    this.setState({
      showConfirmationRollback: true
    });
  };

  onRollbackCancel = () => {
    this.setState({
      showConfirmationRollback: false
    });
  };

  onRollbackCluster = () => {
    if (!this.state.isRollbacking){
      console.log('rollback');

      this.setState({
        isRollbacking: true,
        rollbackSuccessful: false,
        isChangingState: true,
        showConfirmationRollback: false
      });

      Relay.Store.commitUpdate(
        new RollbackClusterMutation(
          {
            releaseId: this.props.releaseInnerId,
          }),
        {
          onSuccess: (response) => {
            console.log('response', response);
            if (response.clusterKitNodeApi_clusterKitNodesApi_releases_rollbackCluster.errors &&
              response.clusterKitNodeApi_clusterKitNodesApi_releases_rollbackCluster.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_rollbackCluster.errors.edges);

              this.setState({
                isRollbacking: false,
                rollbackErrors: messages
              });
            } else {
              console.log('rollback ready', response.clusterKitNodeApi_clusterKitNodesApi_releases_rollbackCluster);
              // total success
              this.setState({
                isRollbacking: false,
                rollbackErrors: null,
                rollbackSuccessful: true,
              });

              this.refetchAfterDelay(5000);
            }
          },
          onFailure: (transaction) => {
            this.setState({
              isRollbacking: false
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
    let rollbackClassName = '';
    if (this.state.isRollbacking) {
      rollbackClassName += ' fa-spin';
    }

    return (
    <div>
      {this.state.rollbackErrors && this.state.rollbackErrors.map((error, index) => {
        return (
          <div className="alert alert-danger" role="alert" key={`error-${index}`}>
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            {' '}
            {error}
          </div>
        );
      })
      }

      {this.state.rollbackSuccessful && !this.state.isChangingState &&
      <div>
        <div className="alert alert-success" role="alert">
          <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
          {' '}
          Rollback successful!
        </div>
      </div>
      }

      {this.props.currentState && this.props.currentState === 'Obsolete' && !this.state.isChangingState &&
      <button className="btn btn-danger" type="button" onClick={this.onRollbackRequestConfirmation.bind(this)}>
        <Icon name="trash" className={rollbackClassName}/>{' '}Rollback cluster to this release
      </button>
      }

      {this.state.isChangingState &&
      <div className="alert alert-warning" role="alert">
        <span className="glyphicon glyphicon-time" aria-hidden="true"></span>
        {' '}
        Please wait, expecting server reply…
      </div>
      }

      {this.state.showConfirmationRollback &&
        <Modal title="Are you sure?" confirmText="Rollback cluster"
               onCancel={this.onRollbackCancel.bind(this)}
               onConfirm={this.onRollbackCluster.bind(this)}
        >
          Rollbacking cluster can lead to the system downtime.
        </Modal>
      }
    </div>
    );
  }
}
