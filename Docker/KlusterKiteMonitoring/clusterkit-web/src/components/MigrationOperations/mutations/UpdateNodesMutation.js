import Relay from 'react-relay'

export default class UpdateNodesMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationNodesUpdate}`
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
      target: this.props.target
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

