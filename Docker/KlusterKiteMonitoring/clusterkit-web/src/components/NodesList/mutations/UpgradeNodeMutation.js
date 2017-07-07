import Relay from 'react-relay'

export default class UpgradeNodeMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{clusterKitNodeApi_clusterKitNodesApi_upgradeNode}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_MutationResult_System_Boolean__MutationPayload {
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
          fragment on ClusterKitNodeApi_MutationResult_System_Boolean__MutationPayload {
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
      address: this.props.address
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

