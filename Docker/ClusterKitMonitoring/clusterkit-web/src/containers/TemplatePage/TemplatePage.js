import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import CreateTemplateMutation from './mutations/CreateTemplateMutation'
import UpdateTemplateMutation from './mutations/UpdateTemplateMutation'
import DeleteTemplateMutation from './mutations/DeleteTemplateMutation'

import TemplateForm from '../../components/TemplateForm/index'

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

  _isAddNew = () => {
    return !this.props.params.hasOwnProperty('id')
  }

  _onSubmit = (model) => {
    console.log('onSubmit!', model);
    console.log('isNew?', this._isAddNew());

    if (this._isAddNew()){
      this._addNode(model);
    } else {
      this._editNode(model);
    }
  }

  _addNode = (model) => {
    Relay.Store.commitUpdate(
      new CreateTemplateMutation(
        {
          code: model.code,
          configuration: model.configuration,
          containerTypes: model.containerTypes,
          maximumNeededInstances: model.maximumNeededInstances,
          minimumRequiredInstances: model.minimumRequiredInstances,
          name: model.name,
          packages: model.packages,
          priority: model.priority,
          version: model.version
        }),
      {
        onSuccess: () => browserHistory.push('/Templates'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  }

  _editNode = (model) => {
    Relay.Store.commitUpdate(
      new UpdateTemplateMutation(
        {
          nodeId: this.props.params.id,
          __id: model.__id,
          code: model.code,
          configuration: model.configuration,
          containerTypes: model.containerTypes,
          maximumNeededInstances: model.maximumNeededInstances,
          minimumRequiredInstances: model.minimumRequiredInstances,
          name: model.name,
          packages: model.packages,
          priority: model.priority,
          version: model.version
        }),
      {
        onSuccess: () => browserHistory.push('/Templates'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  }

  _onDelete = () => {
    console.log('deleting!', this.props.api.__node.__id);

    Relay.Store.commitUpdate(
      new DeleteTemplateMutation({deletedId: this.props.api.__node.__id}),
      {
        onSuccess: () => this.context.router.replace('/Templates'),
        onFailure: (transaction) => console.log(transaction),
      },
    )
  }

  render () {
    const model = this.props.api.__node;
    return (
      <div>
        <TemplateForm onSubmit={this._onSubmit} onDelete={this._onDelete} initialValues={model} />
      </div>
    )
  }
}

export default Relay.createContainer(
  TemplatePage,
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
            ...on ClusterKitNodeApi_ClusterKitNodeTemplate_Node {
              __id
              code
              configuration
              containerTypes
              maximumNeededInstances
              minimumRequiredInstances
              name
              packages
              priority
              version
            }
          }
        }
      `,
    },
  },
)
