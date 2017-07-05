import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import delay from 'lodash/delay'

import Modal from '../Form/Modal'

import CreateMigrationMutation from './mutations/CreateMigrationMutation';
import SetObsoleteMutation from './mutations/SetObsoleteMutation';

import './styles.css';

export class ReadyOperations extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isUpdating: false,
      updateSuccessful: false,
      isSettingObsolete: false,
      setObsoleteSuccessful: false,
      isChangingState: false,
      showConfirmationUpdate: false,
      showConfirmationObsolete: false,
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
      showConfirmationUpdate: true
    });
  };

  onStartMigrationCancel = () => {
    this.setState({
      showConfirmationUpdate: false
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
            if (response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationCreate.errors &&
              response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationCreate.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationCreate.errors.edges);

              this.setState({
                isStartingMigration: false,
                setStableErrors: messages
              });
            } else {
              console.log('result create migration', response.clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationCreate.result);
              // total success
              this.setState({
                isStartingMigration: false,
                setStableErrors: null,
                setStableSuccessful: true,
                showConfirmationUpdate: false
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

  onSetObsoleteConfirmation = () => {
    this.setState({
      showConfirmationObsolete: true
    });
  };

  onSetObsoleteCancel = () => {
    this.setState({
      showConfirmationObsolete: false
    });
  };

  onSetObsolete = () => {
    if (!this.state.isRollbacking){
      console.log('set obsolete');

      this.setState({
        isRollbacking: true,
        rollbackSuccessful: false,
        isChangingState: true
      });

      Relay.Store.commitUpdate(
        new SetObsoleteMutation(
          {
            releaseId: this.props.releaseInnerId,
          }),
        {
          onSuccess: (response) => {
            console.log('response', response);
            if (response.clusterKitNodeApi_clusterKitNodesApi_releases_setObsolete.errors &&
              response.clusterKitNodeApi_clusterKitNodesApi_releases_setObsolete.errors.edges) {
              const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_setObsolete.errors.edges);

              this.setState({
                isRollbacking: false,
                rollbackErrors: messages
              });
            } else {
              console.log('result set obsolete', response.clusterKitNodeApi_clusterKitNodesApi_releases_setObsolete);
              // total success
              this.setState({
                isRollbacking: false,
                rollbackErrors: null,
                rollbackSuccessful: true,
                showConfirmationObsolete: false,
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
    let startMigrationClassName = '';
    if (this.state.isStartingMigration) {
      startMigrationClassName += ' fa-spin';
    }

    let setObsoleteClassName = '';
    if (this.state.isRollbacking) {
      setObsoleteClassName += ' fa-spin';
    }

    return (
    <div>
      {this.state.setStableErrors && this.state.setStableErrors.map((error, index) => {
        return (
          <div className="alert alert-danger" role="alert" key={`error-${index}`}>
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            {' '}
            {error}
          </div>
        );
      })
      }

      {this.state.setStableSuccessful && !this.state.isChangingState &&
      <div>
        <div className="alert alert-success" role="alert">
          <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
          {' '}
          Update cluster successful!
        </div>
      </div>
      }

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
          Set obsolete successful!
        </div>
      </div>
      }

      {this.props.currentState && this.props.currentState === 'Ready' && this.props.canCreateMigration && !this.state.isChangingState &&
      <button className="btn btn-primary" type="button" onClick={this.onStartMigrationConfirmRequest}>
        <Icon name="wrench" className={startMigrationClassName}/>{' '}Start migration
      </button>
      }

      {this.props.currentMigration &&
      <button className="btn btn-primary" type="button" onClick={this.goToMigration}>
        <Icon name="wrench" />{' '}Manage migration
      </button>
      }

      {this.props.currentState && this.props.currentState === 'Ready' && !this.state.isChangingState &&
      <button className="btn btn-danger btn-margined" type="button" onClick={this.onSetObsoleteConfirmation}>
        <Icon name="trash" className={setObsoleteClassName}/>{' '}Set obsolete
      </button>
      }

      {this.state.isChangingState &&
      <div className="alert alert-warning" role="alert">
        <span className="glyphicon glyphicon-time" aria-hidden="true"></span>
        {' '}
        Please wait, expecting server reply…
      </div>
      }

      {this.state.showConfirmationUpdate &&
      <Modal title="Are you sure?" confirmText="Start migration"
             onCancel={this.onStartMigrationCancel.bind(this)}
             onConfirm={this.onStartMigration.bind(this)}
             confirmClass="primary"
      >
        Are you ready to start migration?
      </Modal>
      }

      {this.state.showConfirmationObsolete &&
      <Modal title="Are you sure?" confirmText="Set obsolete"
             onCancel={this.onSetObsoleteCancel.bind(this)}
             onConfirm={this.onSetObsolete.bind(this)}
      >
        You will not be able to use this release after obsoleting it.
      </Modal>
      }
    </div>
    );
  }
}

export default ReadyOperations
