import Relay from 'react-relay'

export default class UpdateResourcesMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationResourceUpdate}`
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
      request: {
        resources: this.props.resources,
      }
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

