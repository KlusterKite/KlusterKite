import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import { Link } from 'react-router';

import ConfigurationOperations from '../../components/ConfigurationOperations/ConfigurationOperations';
import ConfigurationForm from '../../components/ConfigurationForm/ConfigurationForm'
import FeedsList from '../../components/FeedsList/FeedList'
import MigratorTemplatesList from '../../components/MigratorTemplatesList/MigratorTemplatesList'
import NodeTemplatesList from '../../components/NodeTemplatesList/NodeTemplatesList'
import PackagesList from '../../components/PackagesList/PackagesList'
import SeedsList from '../../components/SeedsList/SeedsList'

import CreateConfigurationMutation from './mutations/CreateConfigurationMutation'
import UpdateConfigurationMutation from './mutations/UpdateConfigurationMutation'

class ConfigurationPage extends React.Component {

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
      saving: false,
      saveErrors: null,
    }
  }

  isAddNew = () => {
    return !this.props.params.hasOwnProperty('id')
  };

  onSubmit = (configurationModel) => {
    if (this.isAddNew()){
      this.addNode(configurationModel);
    } else {
      this.editNode(configurationModel);
    }
  };

  onStartMigration = () => {
    browserHistory.push(`/klusterkite/Migration/`);
  };

  addNode = (model) => {
    Relay.Store.commitUpdate(
      new CreateConfigurationMutation(
        {
          majorVersion: model.majorVersion,
          minorVersion: model.minorVersion,
          name: model.name,
          notes: model.notes,
        }),
      {
        onSuccess: (response) => {
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            this.setState({
              saving: false,
              saveErrors: null
            });
            if (this.props.params.mode === 'update') {
              browserHistory.push(`/klusterkite/CopyConfig/${response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.node.id}/update`);
            } else {
              browserHistory.push(`/klusterkite/CopyConfig/${response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.node.id}/exact`);
            }
          }
        },
        onFailure: (transaction) => console.log(transaction),
      },
    )
  };

  editNode = (model) => {
    console.log('saving', model);

    this.setState({
      saving: true
    });

    Relay.Store.commitUpdate(
      new UpdateConfigurationMutation(
        {
          nodeId: this.props.params.id,
          __id: model.__id,
          majorVersion: model.majorVersion,
          minorVersion: model.minorVersion,
          name: model.name,
          notes: model.notes
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_update.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            this.setState({
              saving: false,
              saveErrors: null
            });
            this.props.relay.forceFetch();
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
    console.log('delete', this.props.api.__node.__id);
    // Relay.Store.commitUpdate(
    //   new DeleteFeedMutation({deletedId: this.props.api.__node.__id}),
    //   {
    //     onSuccess: () => this.context.router.replace('/klusterkite/NugetFeeds'),
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  }

  render () {
    const model = this.props.api.configuration;
    const activeConfiguration = this.props.api.activeConfiguration && this.props.api.activeConfiguration.configurations && this.props.api.activeConfiguration.configurations.edges && this.props.api.activeConfiguration.configurations.edges[0].node;
    const nodeManagement = this.props.api.klusterKiteNodesApi.clusterManagement;

    const canEdit = !model || model.state === 'Draft';
    return (
      <div>
        {nodeManagement.currentMigration &&
          <div className="alert alert-warning" role="alert">
            <span className="glyphicon glyphicon-alert" aria-hidden="true"></span>
            {' '}
            Migration is in progress! Please <Link to={'/klusterkite/Migration/'}>finish it</Link>.
          </div>
        }
        <ConfigurationForm
          onSubmit={this.onSubmit}
//          onDelete={this.onDelete}
          initialValues={model}
          saving={this.state.saving}
          saveErrors={this.state.saveErrors}
          canEdit={canEdit}
          activeConfiguration={activeConfiguration}
        />
        {model &&
        <div>
          <ConfigurationOperations
            configuration={model.settings}
            nodeManagement={nodeManagement}
            configurationId={this.props.params.id}
            configurationInnerId={model.__id}
            currentState={model.state}
            onForceFetch={this.props.relay.forceFetch}
            isStable={model.isStable}
            onStartMigration={this.onStartMigration.bind(this)}
          />
          <FeedsList
            configuration={model.settings}
            configurationId={this.props.params.id}
            canEdit={canEdit}
          />
          <NodeTemplatesList
            configuration={model.settings}
            createNodeTemplatePrivilege={true}
            getNodeTemplatePrivilege={true}
            configurationId={this.props.params.id}
            canEdit={canEdit}
          />
          <MigratorTemplatesList
            configuration={model.settings}
            createMigratorTemplatePrivilege={true}
            getMigratorTemplatePrivilege={true}
            configurationId={this.props.params.id}
            canEdit={canEdit}
          />
          <SeedsList
            configuration={model.settings}
            configurationId={this.props.params.id}
            canEdit={canEdit}
          />
          <PackagesList
            configuration={model.settings}
            configurationId={this.props.params.id}
          />
        </div>
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  ConfigurationPage,
  {
    initialVariables: {
      id: null,
      nodeExists: false,
    },
    prepareVariables: (prevVariables) => Object.assign({}, prevVariables, {
      nodeExists: prevVariables.id !== null,
    }),
    fragments: {
      api: () => Relay.QL`
        fragment on IKlusterKiteNodeApi {
          id
          configuration: __node(id: $id) @include( if: $nodeExists ) {
            ...on IKlusterKiteNodeApi_Configuration {
              __id
              name
              notes
              minorVersion
              majorVersion
              state
              isStable
              settings {
                ${FeedsList.getFragment('configuration')},
                ${MigratorTemplatesList.getFragment('configuration')}
                ${NodeTemplatesList.getFragment('configuration')}
                ${PackagesList.getFragment('configuration')}
                ${SeedsList.getFragment('configuration')}
                ${ConfigurationOperations.getFragment('configuration')}
                id
              }
            }
          },
          activeConfiguration: klusterKiteNodesApi {
            configurations (filter: {state: Active}, limit: 1) {
              edges {
                node {
                  minorVersion
                  majorVersion
                }
              }
            }
          },
          klusterKiteNodesApi {
            clusterManagement {
              currentMigration {
                state
              }
              ${ConfigurationOperations.getFragment('nodeManagement')}
            }
          }
        }
      `,
    },
  },
)
