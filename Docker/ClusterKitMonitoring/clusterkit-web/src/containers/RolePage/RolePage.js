import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import RoleForm from '../../components/RoleForm/RoleForm'

import { hasPrivilege } from '../../utils/privileges';

// import CreateReleaseMutation from './mutations/CreateReleaseMutation'
import UpdateRoleMutation from './mutations/UpdateRoleMutation'
// import DeleteFeedMutation from './mutations/DeleteFeedMutation'

class RolePage extends React.Component {

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

    if (this.isAddNew()){
      // this.addNode(releaseModel);
    } else {
      this.editNode(releaseModel);
    }
  };

  addNode = (model) => {
    console.log('create', model);
    // Relay.Store.commitUpdate(
    //   new CreateReleaseMutation(
    //     {
    //       majorVersion: model.majorVersion,
    //       minorVersion: model.minorVersion,
    //       name: model.name,
    //       notes: model.notes,
    //     }),
    //   {
    //     onSuccess: (response) => {
    //       console.log('response', response);
    //       if (response.clusterKitNodeApi_clusterKitNodesApi_releases_create.errors &&
    //         response.clusterKitNodeApi_clusterKitNodesApi_releases_create.errors.edges) {
    //         const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_create.errors.edges);
    //
    //         this.setState({
    //           saving: false,
    //           saveErrors: messages
    //         });
    //       } else {
    //         console.log('success', response);
    //         this.setState({
    //           saving: false,
    //           saveErrors: null
    //         });
    //         browserHistory.push(`/clusterkit/Release/${response.clusterKitNodeApi_clusterKitNodesApi_releases_create.node.id}`);
    //       }
    //     },
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  };

  editNode = (model) => {
    console.log('saving', model);
    console.log('uid', this.props.api.role.uid);

    this.setState({
      saving: true
    });

    Relay.Store.commitUpdate(
      new UpdateRoleMutation(
        {
          nodeId: this.props.params.id,
          uid: this.props.api.role.uid,
          name: model.name,
          allowedScope: model.allowedScope,
          deniedScope: model.deniedScope,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.ClusterKitNodeApi_clusterKitNodesApi_roles_update.errors &&
            response.ClusterKitNodeApi_clusterKitNodesApi_roles_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.ClusterKitNodeApi_clusterKitNodesApi_roles_update.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            this.setState({
              saving: false,
              saveErrors: null
            });
            browserHistory.push(`/clusterkit/Users/`);
          }
        },
        onFailure: (transaction) => {
          this.setState({
            saving: false,
            saveErrors: ['Bad request']
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
    //     onSuccess: () => this.context.router.replace('/clusterkit/NugetFeeds'),
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  }

  render () {
    const model = this.props.api.role;
    const canEdit = hasPrivilege('ClusterKit.NodeManager.User.Update');

    return (
      <div>
        <RoleForm
          onSubmit={this.onSubmit}
          onDelete={this.onDelete}
          initialValues={model}
          saving={this.state.saving}
          saveErrors={this.state.saveErrors}
          canEdit={canEdit}
        />
      </div>
    )
  }
}

export default Relay.createContainer(
  RolePage,
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
        fragment on IClusterKitNodeApi {
          id
          role: __node(id: $id) @include( if: $nodeExists ) {
            ...on IClusterKitNodeApi_Role {
              id
              uid
              name
              allowedScope
              deniedScope
            }
          }
        }
      `,
    },
  },
)
