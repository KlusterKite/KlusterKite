import React from 'react';
import Relay from 'react-relay'

import delay from 'lodash/delay'

import ReloadPackagesMutation from './mutations/ReloadPackagesMutation'

class ReloadPackages extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isReloading: false
    };
  }

  static propTypes = {
    feeds: React.PropTypes.object,
    testMode: React.PropTypes.bool,
  };

  handleReload = () => {
    console.log('Inner reload packages');

    if (this.props.testMode) {
      this.showReloadingMessage();
      this.hideReloadingMessageAfterDelay();
    } else {
      Relay.Store.commitUpdate(
        new ReloadPackagesMutation({}),
        {
          onSuccess: (response) => {
            const result = response.clusterKitNodeApi_nodeManagerData_reloadPackages.result && response.clusterKitNodeApi_nodeManagerData_reloadPackages.result.result;
            if (result) {
              this.showReloadingMessage();
              this.hideReloadingMessageAfterDelay();
            } else {
              this.showErrorMessage();
              this.hideErrorMessageAfterDelay();
            }
          },
          onFailure: (transaction) => console.log(transaction),
        },
      )
    }
  };

  /**
   * Shows reloading packages message
   */
  showReloadingMessage = () => {
    this.setState({
      isReloading: true
    });
  };

  /**
   * Shows reloading packages message
   */
  showErrorMessage = () => {
    this.setState({
      isError: true
    });
  };

  /**
   * Hides reloading packages message after delay
   */
  hideReloadingMessageAfterDelay = () => {
    delay(() => this.hideReloadingMessage(), 5000);
  };

  /**
   * Hides reloading packages message after delay
   */
  hideErrorMessageAfterDelay = () => {
    delay(() => this.hideErrorMessage(), 5000);
  };

  /**
   * Hides reloading packages message
   */
  hideReloadingMessage = () => {
    this.setState({
      isReloading: false
    });
  };

  /**
   * Hides reloading packages message
   */
  hideErrorMessage = () => {
    this.setState({
      isError: false
    });
  };

  render() {
    return (
      <div>
        {this.state.isReloading &&
        <div className="alert alert-success" role="alert">
          <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
          {' '}
          Reloading Packages
        </div>
        }
        {this.state.isError &&
        <div className="alert alert-danger" role="alert">
          <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
          {' '}
          Error Reloading Packages
        </div>
        }
        <button type="button" className="btn btn-primary btn-lg" onClick={this.handleReload}>
          <i className="fa fa-refresh"/> {' '} Reload packages
        </button>
      </div>
    );
  }
}

export default ReloadPackages
