import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import ReleaseOperations from '../../components/ReleaseOperations/ReleaseOperations';
import ReleaseForm from '../../components/ReleaseForm/ReleaseForm'
import FeedsList from '../../components/FeedsList/FeedList'
import MigratorTemplatesList from '../../components/MigratorTemplatesList/MigratorTemplatesList'
import NodeTemplatesList from '../../components/NodeTemplatesList/NodeTemplatesList'
import PackagesList from '../../components/PackagesList/PackagesList'
import SeedsList from '../../components/SeedsList/SeedsList'

import CreateReleaseMutation from './mutations/CreateReleaseMutation'
import UpdateReleaseMutation from './mutations/UpdateReleaseMutation'
// import DeleteFeedMutation from './mutations/DeleteFeedMutation'

class ReleasePage extends React.Component {

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

  onSubmit = (releaseModel) => {
    console.log('submitting release', releaseModel);
    console.log('current model', this.props.api.release);

    if (this.isAddNew()){
      this.addNode(releaseModel);
    } else {
      this.editNode(releaseModel);
    }
  };

  onStartMigration = () => {
    browserHistory.push(`/klusterkite/Migration/`);
  };

  addNode = (model) => {
    console.log('create', model);
    Relay.Store.commitUpdate(
      new CreateReleaseMutation(
        {
          majorVersion: model.majorVersion,
          minorVersion: model.minorVersion,
          name: model.name,
          notes: model.notes,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            console.log('success', response);
            this.setState({
              saving: false,
              saveErrors: null
            });
            browserHistory.push(`/klusterkite/Release/${response.klusterKiteNodeApi_klusterKiteNodesApi_configurations_create.node.id}`);
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
      new UpdateReleaseMutation(
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
    const nodeManagement = this.props.api.klusterKiteNodesApi.clusterManagement;

    const canEdit = !model || model.state === 'Draft';
    return (
      <div>
        <ReleaseForm
          onSubmit={this.onSubmit}
//          onDelete={this.onDelete}
          initialValues={model}
          saving={this.state.saving}
          saveErrors={this.state.saveErrors}
          canEdit={canEdit}
        />
        {model &&
        <div>
          <ReleaseOperations
            configuration={model.settings}
            nodeManagement={nodeManagement}
            releaseId={this.props.params.id}
            releaseInnerId={model.__id}
            currentState={model.state}
            onForceFetch={this.props.relay.forceFetch}
            isStable={model.isStable}
            onStartMigration={this.onStartMigration.bind(this)}
          />
          <FeedsList
            configuration={model.settings}
            releaseId={this.props.params.id}
            canEdit={canEdit}
          />
          <NodeTemplatesList
            configuration={model.settings}
            createNodeTemplatePrivilege={true}
            getNodeTemplatePrivilege={true}
            releaseId={this.props.params.id}
            canEdit={canEdit}
          />
          <MigratorTemplatesList
            configuration={model.settings}
            createMigratorTemplatePrivilege={true}
            getMigratorTemplatePrivilege={true}
            releaseId={this.props.params.id}
            canEdit={canEdit}
          />
          <SeedsList
            configuration={model.settings}
            releaseId={this.props.params.id}
            canEdit={canEdit}
          />
          <PackagesList
            configuration={model.settings}
            releaseId={this.props.params.id}
          />
        </div>
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  ReleasePage,
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
                ${ReleaseOperations.getFragment('configuration')}
                id
              }
            }
          },
          klusterKiteNodesApi {
            clusterManagement {
              ${ReleaseOperations.getFragment('nodeManagement')}
            }
          }
        }
      `,
    },
  },
)
