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
      this.editNode(model, this.props.api.feed.__id);
    }
  };

  addNode = (model) => {
    console.log('createFeed', model);
    // Relay.Store.commitUpdate(
    //   new CreateFeedMutation(
    //     {
    //       clusterKitNodesApiId: this.props.api.clusterKitNodesApi.id,
    //       userName: model.userName,
    //       password: model.password,
    //       address: model.address,
    //       type: model.type,
    //     }),
    //   {
    //     onSuccess: () => browserHistory.push('/clusterkit/NugetFeeds'),
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  };

  editNode = (model, id) => {
    this.setState({
      saving: true
    });

    Relay.Store.commitUpdate(
      new UpdateFeedMutation(
        {
          nodeId: this.props.params.releaseId,
          releaseId: this.props.api.release.__id,
          configuration: this.props.api.release.configuration,
          nugetFeedId: id,
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
            browserHistory.push('/clusterkit/releases');
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
    console.log('delete', this.props.api.feed.__id);
    // Relay.Store.commitUpdate(
    //   new DeleteFeedMutation({deletedId: this.props.api.__node.__id}),
    //   {
    //     onSuccess: () => this.context.router.replace('/clusterkit/NugetFeeds'),
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  };

  onCancel = () => {
    browserHistory.push(`/clusterkit/Release/${this.props.params.releaseId}`)
  };

  render () {
    const model = this.props.api.feed;
    return (
      <div>
        <FeedForm
          onSubmit={this.onSubmit}
          onDelete={this.onDelete}
          onCancel={this.onCancel}
          initialValues={model}
          saving={this.state.saving}
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
          release:__node(id: $releaseId) @include( if: $nodeExists ) {
            ...on IClusterKitNodeApi_Release {
              __id
              configuration {
                ${UpdateFeedMutation.getFragment('configuration')},
              }
            }
          }
          feed:__node(id: $id) @include( if: $nodeExists ) {
            ...on IClusterKitNodeApi_NugetFeed {
              __id
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
