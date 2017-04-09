import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

// import CreateFeedMutation from './mutations/CreateFeedMutation'
import UpdateFeedMutation from './mutations/UpdateFeedMutation'
// import DeleteFeedMutation from './mutations/DeleteFeedMutation'

import FeedForm from '../../components/FeedForm/index'

class FeedPage extends React.Component {

  static propTypes = {
    api: React.PropTypes.object,
    params: React.PropTypes.object,
  };

  static contextTypes = {
    router: React.PropTypes.object,
  };

  constructor (props) {
    super(props);
    this.state = {
    }
  }

  isAddNew = () => {
    return !this.props.params.hasOwnProperty('id')
  };

  onSubmit = (model) => {
    if (this.isAddNew()){
      this.editNode(model, null);
    } else {
      this.editNode(model, this.props.api.feed.id);
    }
  };

  editNode = (model, editId) => {
    this.setState({
      saving: true
    });

    Relay.Store.commitUpdate(
      new UpdateFeedMutation(
        {
          nodeId: this.props.params.releaseId,
          releaseId: this.props.api.release.__id,
          configuration: this.props.api.release.configuration,
          nugetFeedId: editId,
          nugetFeed: {
            userName: model.userName,
            password: model.password,
            address: model.address,
            type: model.type,
          }
        }),
      {
        onSuccess: (response) => {
          if (response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors &&
            response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            browserHistory.push(`/clusterkit/Releases/${this.props.api.release.__id}`);
          }
        },
        onFailure: (transaction) => {
          this.setState({
            saving: false
          });
          console.log(transaction)},
      },
    )
  };

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  }

  onDelete = () => {
    this.setState({
      deleting: true
    });

    Relay.Store.commitUpdate(
      new UpdateFeedMutation(
        {
          nodeId: this.props.params.releaseId,
          releaseId: this.props.api.release.__id,
          configuration: this.props.api.release.configuration,
          nugetFeedId: this.props.api.feed.id,
          nugetFeed: {},
          nugetFeedDeleteId: this.props.api.feed.id,
        }),
      {
        onSuccess: (response) => {
          if (response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors &&
            response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors.edges);

            this.setState({
              deleting: false,
              saveErrors: messages
            });
          } else {
            browserHistory.push(`/clusterkit/Releases/${this.props.api.release.__id}`);
          }
        },
        onFailure: (transaction) => {
          this.setState({
            deleting: false
          });
          console.log(transaction)},
      },
    )
  };

  onCancel = () => {
    browserHistory.push(`/clusterkit/Releases/${this.props.params.releaseId}`)
  };

  render () {
    const model = this.props.api.feed;
    return (
      <div>
        <FeedForm
          onSubmit={this.onSubmit}
          onDelete={model && this.onDelete}
          onCancel={this.onCancel}
          initialValues={model}
          saving={this.state.saving}
          deleting={this.state.deleting}
          saveErrors={this.state.saveErrors}
        />
      </div>
    )
  }
}

export default Relay.createContainer(
  FeedPage,
  {
    initialVariables: {
      id: null,
      releaseId: null,
      nodeExists: false,
    },
    prepareVariables: (prevVariables) => Object.assign({}, prevVariables, {
      nodeExists: prevVariables.id !== null,
    }),
    fragments: {
      api: () => Relay.QL`
        fragment on IClusterKitNodeApi {
          __typename
          id
          release:__node(id: $releaseId) {
            ...on IClusterKitNodeApi_Release {
              __id
              configuration {
                ${UpdateFeedMutation.getFragment('configuration')},
              }
            }
          }
          feed:__node(id: $id) @include( if: $nodeExists ) {
            ...on IClusterKitNodeApi_NugetFeed {
              id
              address
              type
              userName
              password
            }
          }
        }
      `,
    },
  },
)
