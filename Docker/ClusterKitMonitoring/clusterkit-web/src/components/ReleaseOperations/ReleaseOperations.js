import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';
import { Link } from 'react-router';

import CheckReleaseMutation from './mutations/CheckReleaseMutation';
import SetReadyMutation from './mutations/SetReadyMutation';
import UpdateClusterMutation from './mutations/UpdateClusterMutation';

import CheckReleaseResult from './CheckReleaseResult';


import './styles.css';

export class ReleaseOperations extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isChecking: false,
      checkSuccess: false,
      checkCompatibleTemplates: null,
      checkActiveNodes: null,
      isSettingReady: false,
      isUpdating: false,
    };
  }

  static propTypes = {
    configuration: React.PropTypes.object,
    releaseId: React.PropTypes.string.isRequired,
    releaseInnerId: React.PropTypes.number.isRequired,
    currentState: React.PropTypes.string.isRequired,
  };

  onCheck = () => {
    console.log('checking release');

    this.setState({
      isChecking: true,
      checkSuccess: false
    });

    Relay.Store.commitUpdate(
      new CheckReleaseMutation(
        {
          releaseId: this.props.releaseInnerId,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.clusterKitNodeApi_clusterKitNodesApi_releases_check.errors &&
            response.clusterKitNodeApi_clusterKitNodesApi_releases_check.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_check.errors.edges);

            this.setState({
              isChecking: false,
              checkErrors: messages
            });
          } else {
            console.log('result check', response.clusterKitNodeApi_clusterKitNodesApi_releases_check);
            // total success
            this.setState({
              isChecking: false,
              checkErrors: null,
              checkSuccess: true,
              checkCompatibleTemplates: response.clusterKitNodeApi_clusterKitNodesApi_releases_check.node.compatibleTemplates.edges,
              checkActiveNodes: response.clusterKitNodeApi_clusterKitNodesApi_releases_check.api.clusterKitNodesApi.getActiveNodeDescriptions.edges,
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
  };

  onSetReady = () => {
    console.log('set ready');

    this.setState({
      isSettingReady: true,
      setReadySuccessful: false
    });

    Relay.Store.commitUpdate(
      new SetReadyMutation(
        {
          releaseId: this.props.releaseInnerId,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.clusterKitNodeApi_clusterKitNodesApi_releases_setReady.errors &&
            response.clusterKitNodeApi_clusterKitNodesApi_releases_setReady.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_setReady.errors.edges);

            this.setState({
              isSettingReady: false,
              setReadyErrors: messages
            });
          } else {
            console.log('result set ready', response.clusterKitNodeApi_clusterKitNodesApi_releases_setReady);
            // total success
            this.setState({
              isSettingReady: false,
              setReadyErrors: null,
              setReadySuccessful: true,
            });

            this.props.relay.forceFetch();
          }
        },
        onFailure: (transaction) => {
          this.setState({
            isSettingReady: false
          });
          console.log(transaction)},
      },
    );
  };

  onUpdateCluster = () => {
    console.log('update cluster');

    this.setState({
      isUpdating: true,
      updateSuccessful: false
    });

    Relay.Store.commitUpdate(
      new UpdateClusterMutation(
        {
          releaseId: this.props.releaseInnerId,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.clusterKitNodeApi_clusterKitNodesApi_releases_updateCluster.errors &&
            response.clusterKitNodeApi_clusterKitNodesApi_releases_updateCluster.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_updateCluster.errors.edges);

            this.setState({
              isUpdating: false,
              updateErrors: messages
            });
          } else {
            console.log('result set ready', response.clusterKitNodeApi_clusterKitNodesApi_releases_updateCluster);
            // total success
            this.setState({
              isUpdating: false,
              updateErrors: null,
              updateSuccessful: true,
            });

            this.props.relay.forceFetch();
          }
        },
        onFailure: (transaction) => {
          this.setState({
            isUpdating: false
          });
          console.log(transaction)},
      },
    );
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

    let updateClusterClassName = '';
    if (this.state.isUpdating) {
      updateClusterClassName += ' fa-spin';
    }

    return (
      <div>
        <h3>Release Operations</h3>
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

          {this.state.updateErrors && this.state.updateErrors.map((error, index) => {
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

              <CheckReleaseResult
                activeNodes={this.state.checkActiveNodes}
                compatibleTemplates={this.state.checkCompatibleTemplates}
                newNodeTemplates={this.props.configuration.nodeTemplates.edges}
                newReleaseInnerId={this.props.releaseInnerId}
              />
            </div>
          }

          {this.state.setReadySuccessful &&
          <div>
            <div className="alert alert-success" role="alert">
              <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
              {' '}
              Set ready successful!
            </div>
          </div>
          }

          {this.state.updateSuccessful &&
          <div>
            <div className="alert alert-success" role="alert">
              <span className="glyphicon glyphicon-ok" aria-hidden="true"></span>
              {' '}
              Update cluster successful!
            </div>
          </div>
          }

          <p>State: {this.props.currentState}</p>

          {this.props.currentState && this.props.currentState === 'Draft' &&
            <button className="btn btn-default" type="button" onClick={this.onCheck}>
              <Icon name="check-circle" className={checkClassName}/>{' '}Check release
            </button>
          }

          {this.props.currentState && this.props.currentState === 'Draft' &&
            <button className="btn btn-default btn-margined" type="button" onClick={this.onSetReady}>
              <Icon name="flag-checkered" className={setReadyClassName}/>{' '}Set ready
            </button>
          }

          {this.props.currentState && this.props.currentState === 'Ready' &&
          <button className="btn btn-default" type="button" onClick={this.onUpdateCluster}>
            <Icon name="wrench" className={updateClusterClassName}/>{' '}Update cluster
          </button>
          }

        </div>
        {this.props.currentState && this.props.currentState === 'Draft' &&
          <div className="buttons-block-margin">
            <Link to={`/clusterkit/CopyConfig/${this.props.releaseId}/update`} className="btn btn-success" role="button">
              <Icon name="clone"/>{' '}Clone configuration (update packages)
            </Link>

            <Link to={`/clusterkit/CopyConfig/${this.props.releaseId}/exact`} className="btn btn-success btn-margined"
                  role="button">
              <Icon name="clone"/>{' '}Clone configuration (exact)
            </Link>
          </div>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  ReleaseOperations,
  {
    fragments: {
      configuration: () => Relay.QL`fragment on IClusterKitNodeApi_ReleaseConfiguration {
        nodeTemplates {
          edges {
            node {
              code
            }
          }
        }
      }
      `,
    },
  },
)
