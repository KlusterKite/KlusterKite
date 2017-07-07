import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import UserForm from '../../components/UserForm/UserForm'

import { hasPrivilege } from '../../utils/privileges';

// import CreateReleaseMutation from './mutations/CreateReleaseMutation'
import UpdateUserMutation from './mutations/UpdateUserMutation'
// import DeleteFeedMutation from './mutations/DeleteFeedMutation'

class UserPage extends React.Component {

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
      // this.addNode(releaseModel);
    } else {
      // this.editNode(releaseModel);
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
    //       if (response.klusterKiteNodeApi_klusterKiteNodesApi_releases_create.errors &&
    //         response.klusterKiteNodeApi_klusterKiteNodesApi_releases_create.errors.edges) {
    //         const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_releases_create.errors.edges);
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
    //         browserHistory.push(`/klusterkite/Release/${response.klusterKiteNodeApi_klusterKiteNodesApi_releases_create.node.id}`);
    //       }
    //     },
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  };

  editNode = (model) => {
    console.log('saving', model);

    this.setState({
      saving: true
    });

    Relay.Store.commitUpdate(
      new UpdateUserMutation(
        {
          nodeId: this.props.params.id,
          uid: model.uid,
          login: model.login,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_users_update.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_users_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_users_update.errors.edges);

            this.setState({
              saving: false,
              saveErrors: messages
            });
          } else {
            this.setState({
              saving: false,
              saveErrors: null
            });
            browserHistory.push(`/klusterkite/Users/`);
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
    //     onSuccess: () => this.context.router.replace('/klusterkite/NugetFeeds'),
    //     onFailure: (transaction) => console.log(transaction),
    //   },
    // )
  }

  render () {
    const model = this.props.api.user;
    const canEdit = hasPrivilege('KlusterKite.NodeManager.User.Update');
    const roles = this.props.api.klusterKiteNodesApi.roles && this.props.api.klusterKiteNodesApi.roles.edges.map(x => x.node.name);

    return (
      <div>
        <UserForm
          onSubmit={this.onSubmit}
          onDelete={this.onDelete}
          initialValues={model}
          saving={this.state.saving}
          saveErrors={this.state.saveErrors}
          canEdit={canEdit}
          roles={roles}
        />
      </div>
    )
  }
}

export default Relay.createContainer(
  UserPage,
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
          user: __node(id: $id) @include( if: $nodeExists ) {
            ...on IKlusterKiteNodeApi_User {
              id
              uid
              login
              roles {
                edges {
                  node {
                    name
                  }
                }
              }
            }
          }
          klusterKiteNodesApi {
            roles {
              edges {
                node {
                  name
                }
              }
            }
          }
        }
      `,
    },
  },
)
