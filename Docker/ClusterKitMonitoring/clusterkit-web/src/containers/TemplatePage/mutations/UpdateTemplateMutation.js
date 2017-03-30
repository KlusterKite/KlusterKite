import Relay from 'react-relay'

export default class UpdateTemplateMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_nodeManagerData_nodeTemplates_update}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_ClusterKitNodeTemplate_NodeMutationPayload {
        node
        api {
          nodeManagerData {
            nugetFeeds
          }
        }
      }
    `
  }

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
      model: {
        id: this.props.nodeId,
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
    }
  }
}

