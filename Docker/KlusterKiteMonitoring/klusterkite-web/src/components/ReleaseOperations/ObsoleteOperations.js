import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';
import delay from 'lodash/delay'

import Modal from '../Form/Modal'

import CreateMigrationMutation from './mutations/CreateMigrationMutation';

import './styles.css';

export default class ObsoleteOperations extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isRollbacking: false,
      setObsoleteSuccessful: false,
      isChangingState: false,
      showConfirmationRollback: false,
    };
  }

  static propTypes = {
    releaseId: React.PropTypes.string.isRequired,
    releaseInnerId: React.PropTypes.number.isRequired,
    currentState: React.PropTypes.string.isRequired,
    onForceFetch: React.PropTypes.func.isRequired,
    canCreateMigration: React.PropTypes.bool.isRequired,
    onStartMigration: React.PropTypes.func.isRequired,
    currentMigration: React.PropTypes.object,
  };

  onStartMigrationConfirmRequest = () => {
    this.setState({
      showConfirmationRollback: true
    });
  };

  onStartMigrationCancel = () => {
    this.setState({
      showConfirmationRollback: false
    });
  };

  onStartMigration = () => {
    if (!this.state.isStartingMigration){

      this.setState({
        isStartingMigration: true,
        setStableSuccessful: false,
        isChangingState: true
      });

      Relay.Store.commitUpdate(
        new CreateMigrationMutation(
          {
            releaseId: this.props.releaseInnerId,
          }),
        {
          onSuccess: (response) => {
            console.log('response', response);
            if (response.klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationCreate.errors &&
              response.klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationCreate.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationCreate.errors.edges);

              this.setState({
                isStartingMigration: false,
                setStableErrors: messages
              });
            } else {
              console.log('result create migration', response.klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationCreate.result);
              // total success
              this.setState({
                isStartingMigration: false,
                setStableErrors: null,
                setStableSuccessful: true,
                showConfirmationRollback: false
              });

              this.props.onStartMigration();
            }
          },
          onFailure: (transaction) => {
            this.setState({
              isStartingMigration: false
            });
            console.log(transaction)},
        },
      );
    }
  };

  goToMigration = () => {
    this.props.onStartMigration();
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
    let startMigrationClassName = '';
    if (this.state.isStartingMigration) {
      startMigrationClassName += ' fa-spin';
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

      {this.state.setObsoleteSuccessful && !this.state.isChangingState &&
      <div>
        <div className="alert alert-success" role="alert">
          <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
          {' '}
          Rollback successful!
        </div>
      </div>
      }

      {this.props.currentState && this.props.currentState === 'Obsolete' && this.props.canCreateMigration && !this.state.isChangingState &&
      <button className="btn btn-primary" type="button" onClick={this.onStartMigrationConfirmRequest}>
        <Icon name="wrench" className={startMigrationClassName}/>{' '}Start rollback migration
      </button>
      }

      {this.props.currentMigration &&
      <button className="btn btn-primary" type="button" onClick={this.goToMigration}>
        <Icon name="wrench" />{' '}Manage migration
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
      <Modal title="Are you sure?" confirmText="Start migration"
             onCancel={this.onStartMigrationCancel.bind(this)}
             onConfirm={this.onStartMigration.bind(this)}
             confirmClass="primary"
      >
        Are you ready to start migration?
      </Modal>
      }
    </div>
    );
  }
}
