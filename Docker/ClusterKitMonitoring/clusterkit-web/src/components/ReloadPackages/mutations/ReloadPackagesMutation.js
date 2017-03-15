import Relay from 'react-relay'

export default class ReloadPackagesMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_nodeManagerData_reloadPackages}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_ClusterKit_API_Client_MutationResult_System_Boolean__MutationPayload {
        result
        api
      }
    `
  }

  getConfigs () {
    return [{
      type: 'FIELDS_CHANGE',
      fieldIDs: {
        result: this.props.result,
      },
    }]
  }

  getVariables () {
    return {
    }
  }

  getOptimisticResponse () {
    return {
      result : true
    }
  }
}

