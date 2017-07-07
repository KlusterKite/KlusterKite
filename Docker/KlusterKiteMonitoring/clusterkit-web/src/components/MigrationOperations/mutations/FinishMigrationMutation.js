import Relay from 'react-relay'

export default class FinishMigrationMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationFinish}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on Boolean_MutationPayload {
        result
      }
    `
  }

  getConfigs() {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
          Relay.QL`
          fragment on Boolean_MutationPayload {
            result
          }
        `,
      ],
    }];
  }

  getVariables () {
    return {}
  }

  getOptimisticResponse () {
    return {
      result: {
        result: true
      }
    }
  }
}

