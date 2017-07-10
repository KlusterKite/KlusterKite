import Relay from 'react-relay'

export default class UpgradeNodeMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_upgradeNode}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_MutationResult_System_Boolean__MutationPayload {
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
          fragment on KlusterKiteNodeApi_MutationResult_System_Boolean__MutationPayload {
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

