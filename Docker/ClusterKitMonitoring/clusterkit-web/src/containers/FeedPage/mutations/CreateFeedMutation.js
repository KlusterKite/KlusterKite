import Relay from 'react-relay'

export default class CreateFeedMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_clusterKitNodesApi_nugetFeeds_create}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_NugetFeed_NodeMutationPayload {
        node
        edge
        api {
          clusterKitNodesApi {
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
  //     parentID: this.props.clusterKitNodesApiId,
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
        address: this.props.address,
        userName: this.props.userName,
        password: this.props.password,
        type: this.props.type,
      }
    }
  }

  getOptimisticResponse () {
    return {
      edge: {
        node: {
          address: this.props.address,
          userName: this.props.userName,
          password: this.props.password,
          type: this.props.type,
        },
      },
    }
  }
}

