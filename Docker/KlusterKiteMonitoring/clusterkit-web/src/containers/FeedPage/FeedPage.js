import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import UpdateFeedMutation from './mutations/UpdateFeedMutation'

import FeedForm from '../../components/FeedForm/FeedForm'

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

  onSubmit = (model) => {
    this.editNode(model);
  };

  editNode = (model) => {
    this.setState({
      saving: true
    });

    Relay.Store.commitUpdate(
      new UpdateFeedMutation(
        {
          nodeId: this.props.params.releaseId,
          releaseId: this.props.api.release.__id,
          configuration: this.props.api.release.configuration,
          nugetFeed: model.nugetFeed,
        }),
      {
        onSuccess: (response) => {
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_releases_update.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_releases_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_releases_update.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            browserHistory.push(`/klusterkite/Release/${this.props.params.releaseId}`);
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

  onCancel = () => {
    browserHistory.push(`/klusterkite/Release/${this.props.params.releaseId}`)
  };

  render () {
    console.log('render', this.props.api);
    const model = {
      nugetFeed: this.props.api.release.configuration.nugetFeed,
    };

    return (
      <div>
        <FeedForm
          onSubmit={this.onSubmit}
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
        fragment on IKlusterKiteNodeApi {
          __typename
          id
          release:__node(id: $releaseId) {
            ...on IKlusterKiteNodeApi_Release {
              __id
              configuration {
                ${UpdateFeedMutation.getFragment('configuration')},
                nugetFeed,
              }
            }
          }
        }
      `,
    },
  },
)
