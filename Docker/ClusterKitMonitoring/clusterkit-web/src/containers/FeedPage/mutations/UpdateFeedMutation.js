import Relay from 'react-relay'

export default class UpdateFeedMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_nodeManagerData_nugetFeeds_update}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_ClusterKitNugetFeed_NodeMutationPayload {
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
        address: this.props.address,
        userName: this.props.userName,
        password: this.props.password,
        type: this.props.type,
      }
    }
  }

  getOptimisticResponse () {
    return {
      model: {
        id: this.props.nodeId,
        address: this.props.address,
        userName: this.props.userName,
        password: this.props.password,
        type: this.props.type,
      },
    }
  }
}

