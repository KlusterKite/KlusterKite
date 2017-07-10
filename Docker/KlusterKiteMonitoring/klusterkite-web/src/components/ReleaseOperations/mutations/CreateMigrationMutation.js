import Relay from 'react-relay'

export default class CreateMigrationMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_clusterManagement_migrationCreate}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_MutationResult_Migration__MutationPayload {
        result {
          result {
            state
          }
          errors {
            edges {
              node {
                field
                message
              }
            }
          }
        }
      }
    `
  }

  getConfigs() {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
          Relay.QL`
          fragment on KlusterKiteNodeApi_MutationResult_Migration__MutationPayload {
            result {
              result {
                state
              }
              errors {
                edges {
                  node {
                    field
                    message
                  }
                }
              }
            }
          }
        `,
      ],
    }];
  }

  getVariables () {
    return {
      newReleaseId: this.props.releaseId
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

