import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import UpdateFeedMutation from '../FeedPage/mutations/UpdateFeedMutation'

import SeedForm from '../../components/SeedForm/SeedForm'

class SeedPage extends React.Component {

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
    console.log('on submit', model);

    this.editNode(model, null);
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
          seedAddresses: model.seedAddresses
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
            console.log('success');
            // browserHistory.push(`/clusterkit/Release/${this.props.params.releaseId}`);
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
    browserHistory.push(`/clusterkit/Release/${this.props.params.releaseId}`)
  };

  render () {
    const model = this.props.api.release.configuration.seedAddresses;
    return (
      <div>
        <SeedForm
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
  SeedPage,
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
                seedAddresses
              }
            }
          }
        }
      `,
    },
  },
)
