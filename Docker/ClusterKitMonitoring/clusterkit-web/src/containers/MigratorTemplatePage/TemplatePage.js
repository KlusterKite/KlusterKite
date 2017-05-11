import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import UpdateFeedMutation from './../FeedPage/mutations/UpdateFeedMutation'

import MigratorTemplateForm from '../../components/MigratorTemplateForm/MigratorTemplateForm'

class TemplatePage extends React.Component {

  static propTypes = {
    api: React.PropTypes.object,
    params: React.PropTypes.object,
  }

  static contextTypes = {
    router: React.PropTypes.object,
  }

  constructor (props) {
    super(props)
    this.state = {
    }
  }

  isAddNew = () => {
    return !this.props.params.hasOwnProperty('id')
  }

  onSubmit = (model) => {
    if (this.isAddNew()){
      this.editNode(model, null);
    } else {
      this.editNode(model, this.props.api.template.id);
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
          migratorTemplateId: editId,
          migratorTemplate: model
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
            browserHistory.push(`/clusterkit/Releases/${this.props.params.releaseId}`);
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
          migratorTemplateId: this.props.api.release.id,
          migratorTemplate: {},
          migratorTemplateDeleteId: this.props.params.id,
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
            browserHistory.push(`/clusterkit/Releases/${this.props.params.releaseId}`);
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
    const model = this.props.api.template;
    const packages = this.props.api.clusterKitNodesApi.nugetPackages;
    return (
      <div>
        <MigratorTemplateForm
          onSubmit={this.onSubmit}
          onDelete={this.onDelete}
          onCancel={this.onCancel}
          initialValues={model}
          packagesList={packages}
          saving={this.state.saving}
          deleting={this.state.deleting}
          saveErrors={this.state.saveErrors}
        />
      </div>
    )
  }
}

export default Relay.createContainer(
  TemplatePage,
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
          id
          __typename
          clusterKitNodesApi {
            nugetPackages {
              edges {
                node {
                  name
                  version
                  availableVersions
                }
              }
            }
          }
          release:__node(id: $releaseId) {
            ...on IClusterKitNodeApi_Release {
              __id
              configuration {
                ${UpdateFeedMutation.getFragment('configuration')},
              }
            }
          }
          template:__node(id: $id) @include( if: $nodeExists ) {
            ...on IClusterKitNodeApi_MigratorTemplate {
              id
              code
              configuration
              name
              notes
              packageRequirements {
                edges {
                  node {
                    __id
                    specificVersion
                  }
                }
              },
              priority
            }
          }
        }
      `,
    },
  },
)
