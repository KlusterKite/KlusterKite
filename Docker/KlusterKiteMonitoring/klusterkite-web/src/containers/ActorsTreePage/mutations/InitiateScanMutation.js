import Relay from 'react-relay'

export default class InitiateScanMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteMonitoring_klusterKiteMonitoringApi_initiateScan}`
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
    return {
    }
  }

  getOptimisticResponse () {
    return {
      result: true
    }
  }
}

