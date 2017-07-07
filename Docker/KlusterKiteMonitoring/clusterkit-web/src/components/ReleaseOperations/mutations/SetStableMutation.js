import Relay from 'react-relay'

export default class SetStableMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_releases_setStable}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_Release_NodeMutationPayload {
        node
        edge
        errors {
          edges {
            node {
              field
              message
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
          fragment on KlusterKiteNodeApi_Release_NodeMutationPayload {
            errors {
              edges {
                node {
                  field
                  message
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
      id: this.props.releaseId,
      isStable: this.props.isStable,
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

