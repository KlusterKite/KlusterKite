import React from 'react';
import Relay from 'react-relay'
import Icon from 'react-fa';

import { Link } from 'react-router';

import CheckReleaseMutation from './mutations/CheckReleaseMutation';

export default class ReleaseOperations extends React.Component {
  constructor(props) {
    super(props);

    this.state = {};
  }

  static propTypes = {
    releaseId: React.PropTypes.string.isRequired,
    releaseInnerId: React.PropTypes.number.isRequired,
  };

  onCheck() {
    console.log('checking release');
  }

  onCheck = () => {
    this.setState({
      checking: true
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
              checking: false,
              checkErrors: messages
            });
          } else {
            // total success
          }
        },
        onFailure: (transaction) => {
          this.setState({
            checking: false
          });
          console.log(transaction)},
      },
    )
  };

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  };

  render() {
    let checkClassName = '';
    if (this.props.checking) {
      checkClassName += ' fa-spin';
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

          <button className="btn btn-default" type="button" onClick={this.onCheck}>
            <Icon name="circle-thin" className={checkClassName} />{' '}Check release
          </button>

          <Link to={`/clusterkit/CopyConfig/${this.props.releaseId}`} className="btn btn-default btn-margined" role="button">
            <Icon name="clone" />{' '}Clone configuration from active release
          </Link>
        </div>
      </div>
    );
  }
}

