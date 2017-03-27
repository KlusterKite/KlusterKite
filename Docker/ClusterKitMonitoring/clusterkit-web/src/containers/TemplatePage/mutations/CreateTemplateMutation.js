import Relay from 'react-relay'

export default class CreateTemplateMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_nodeManagerData_nodeTemplates_create}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_ClusterKitNodeTemplate_NodeMutationPayload {
        node
        edge
        api {
          nodeManagerData {
            nugetFeeds
          }
        }
      }
    `
  }

  // getConfigs () {
  //   return [{
  //     type: 'RANGE_ADD',
  //     parentName: 'nodeManagerData',
  //     parentID: null,
  //     connectionName: 'nugetFeeds',
  //     edgeName: 'edge',
  //     rangeBehaviors: {
  //       '': 'append',
  //     },
  //   }]
  // }

  getConfigs () {
    return [{
      type: 'FIELDS_CHANGE',
      fieldIDs: {
        node: this.props.nodeId,
      },
    }]
  }

  getVariables () {
    return {
      id: this.props.__id,
      newNode: {
        id: this.props.__id,
        code: this.props.code,
        configuration: this.props.configuration,
        containerTypes: this.props.containerTypes,
        maximumNeededInstances: this.props.maximumNeededInstances,
        minimumRequiredInstances: this.props.minimumRequiredInstances,
        name: this.props.name,
        packages: this.props.packages,
        priority: this.props.priority,
        version: this.props.version,
      }
    }
  }

  getOptimisticResponse () {
    return {
      edge: {
        node: {
          code: this.props.code,
          configuration: this.props.configuration,
          containerTypes: this.props.containerTypes,
          maximumNeededInstances: this.props.maximumNeededInstances,
          minimumRequiredInstances: this.props.minimumRequiredInstances,
          name: this.props.name,
          packages: this.props.packages,
          priority: this.props.priority,
          version: this.props.version,
        },
      },
    }
  }
}

