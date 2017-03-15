import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import CreateFeedMutation from './mutations/CreateFeedMutation'
import UpdateFeedMutation from './mutations/UpdateFeedMutation'
import DeleteFeedMutation from './mutations/DeleteFeedMutation'

import FeedForm from '../../components/FeedForm/index'

class FeedPage extends React.Component {

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

  _isAddNew = () => {
    return !this.props.params.hasOwnProperty('id')
  }

  _onSubmit = (model) => {
    console.log('onSubmit!', model);
    console.log(this._isAddNew());

    if (this._isAddNew()){
      this._addNode(model);
    } else {
      this._editNode(model);
    }
  }

  _addNode = (model) => {
    Relay.Store.commitUpdate(
      new CreateFeedMutation(
        {
          userName: model.userName,
          password: model.password,
          address: model.address,
          type: model.type,
        }),
      {
        onSuccess: () => browserHistory.push('/NugetFeeds'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  }

  _editNode = (model) => {
    Relay.Store.commitUpdate(
      new UpdateFeedMutation(
        {
          nodeId: this.props.params.id,
          __id: model.__id,
          userName: model.userName,
          password: model.password,
          address: model.address,
          type: model.type,
        }),
      {
        onSuccess: () => browserHistory.push('/NugetFeeds'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  }

  _onDelete = () => {
    console.log('deleting!', this.props.api.__node.__id);

    Relay.Store.commitUpdate(
      new DeleteFeedMutation({deletedId: this.props.api.__node.__id}),
      {
        onSuccess: () => this.context.router.replace('/NugetFeeds'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  }

  render () {
    const model = this.props.api.__node;
    return (
      <div>
        <FeedForm onSubmit={this._onSubmit} onDelete={this._onDelete} initialValues={model} />
      </div>
    )
  }
}

export default Relay.createContainer(
  FeedPage,
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
        fragment on ClusterKitNodeApi_ClusterKitNodeApi {
          id
          __node(id: $id) @include( if: $nodeExists ) {
            ...on ClusterKitNodeApi_ClusterKitNugetFeed_Node {
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
