import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import UpdateFeedMutation from './../FeedPage/mutations/UpdateFeedMutation'

import NodeTemplateForm from '../../components/NodeTemplateForm/NodeTemplateForm'

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
          nodeId: this.props.params.configurationId,
          configurationId: this.props.api.configuration.__id,
          settings: this.props.api.configuration.settings,
          nodeTemplateId: editId,
          nodeTemplate: model
        }),
      {
        onSuccess: (response) => {
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            browserHistory.push(`/klusterkite/Configuration/${this.props.params.configurationId}`);
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
          nodeId: this.props.params.configurationId,
          configurationId: this.props.api.configuration.__id,
          settings: this.props.api.configuration.settings,
          nodeTemplateId: this.props.api.configuration.id,
          nodeTemplate: {},
          nodeTemplateDeleteId: this.props.params.id,
        }),
      {
        onSuccess: (response) => {
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors.edges);

            this.setState({
              deleting: false,
              saveErrors: messages
            });
          } else {
            browserHistory.push(`/klusterkite/Configuration/${this.props.params.configurationId}`);
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
    browserHistory.push(`/klusterkite/Configuration/${this.props.params.configurationId}`)
  };

  render () {
    const model = this.props.api.template;
    const packages = this.props.api.klusterKiteNodesApi.nugetPackages;
    return (
      <div>
        <NodeTemplateForm
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
      configurationId: null,
      nodeExists: false,
    },
    prepareVariables: (prevVariables) => Object.assign({}, prevVariables, {
      nodeExists: prevVariables.id !== null,
    }),
    fragments: {
      api: () => Relay.QL`
        fragment on IKlusterKiteNodeApi {
          id
          __typename
          klusterKiteNodesApi {
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
          configuration:__node(id: $configurationId) {
            ...on IKlusterKiteNodeApi_Configuration {
              __id
              settings {
                ${UpdateFeedMutation.getFragment('settings')},
              }
            }
          }
          template:__node(id: $id) @include( if: $nodeExists ) {
            ...on IKlusterKiteNodeApi_NodeTemplate {
              id
              code
              configuration
              containerTypes
              maximumNeededInstances
              minimumRequiredInstances
              name
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
