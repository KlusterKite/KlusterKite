import Relay from 'react-relay'

export default class ReloadPackagesMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_nodeManagerData_reloadPackages}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_ClusterKit_API_Client_MutationResult_System_Boolean__MutationPayload {
        result {
          result
        }
      }
    `
  }

  getConfigs() {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
          Relay.QL`
          fragment on ClusterKitNodeApi_ClusterKit_API_Client_MutationResult_System_Boolean__MutationPayload {
            result {
              result
            }
          }
        `,
      ],
    }];
  }

  getVariables () {
    return {
    }
  }

  getOptimisticResponse () {
    return {
      result: {
        result: true
      }
    }
  }
}

