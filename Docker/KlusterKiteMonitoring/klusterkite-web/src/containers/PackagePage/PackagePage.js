import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import UpdateFeedMutation from '../FeedPage/mutations/UpdateFeedMutation'

import SinglePackageForm from '../../components/PackageForm/SinglePackageForm'

class PackagePage extends React.Component {

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
          nodeId: this.props.params.configurationId,
          configurationId: this.props.api.configuration.__id,
          settings: this.props.api.configuration.settings,
          packages: this.props.api.configuration.settings.packages,
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

  onCancel = () => {
    browserHistory.push(`/klusterkite/Configuration/${this.props.params.configurationId}`)
  };

  render () {
    const packages = this.props.api.klusterKiteNodesApi.nugetPackages;
    const model = {
      package: {
        id: this.props.api.package.__id,
        specificVersion: this.props.api.package.version
      },
    };

    return (
      <div>
        <SinglePackageForm
          onSubmit={this.onSubmit}
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
  PackagePage,
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
          __typename
          id
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
                nugetFeed,
              }
            }
          }
          package:__node(id: $id) @include( if: $nodeExists ) {
            ...on IKlusterKiteNodeApi_PackageDescription {
              id
              version
              __id
            }
          }
        }
      `,
    },
  },
)
