import Relay from 'react-relay'

export default class DeleteFeedMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_clusterKitNodesApi_nugetFeeds_delete}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_NugetFeed_NodeMutationPayload {
        deletedId
        node
      }
    `
  }

  // getConfigs () {
  //   return [{
  //     type: 'NODE_DELETE',
  //     parentName: 'viewer',
  //     parentID: this.props.viewerId,
  //     connectionName: 'pokemon',
  //     deletedIDFieldName: 'deletedId',
  //   }]
  // }

  getConfigs () {
    return [{
      type: 'FIELDS_CHANGE',
      fieldIDs: {
        node: this.props.deletedId,
      },
    }]
  }

  getVariables () {
    return {
      id: this.props.deletedId,
    }
  }

  getOptimisticResponse () {
    return {
      deletedId: this.props.deletedId,
    }
  }
}

