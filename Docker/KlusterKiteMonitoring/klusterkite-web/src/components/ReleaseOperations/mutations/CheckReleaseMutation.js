import Relay from 'react-relay'

export default class CheckReleaseMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_configurations_check}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_Configuration_NodeMutationPayload {
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
          fragment on KlusterKiteNodeApi_Configuration_NodeMutationPayload {
            errors {
              edges {
                node {
                  field
                  message
                }
              }
            }
            node {
              compatibleTemplatesForward {
                edges {
                  node {
                    templateCode
                    releaseId
                    compatibleReleaseId
                  }
                }
              }
              compatibleTemplatesBackward {
                edges {
                  node {
                    templateCode
                    releaseId
                    compatibleReleaseId
                  }
                }
              }
            }
            api {
              klusterKiteNodesApi {
                getActiveNodeDescriptions {
                  edges {
                    node {
                      nodeTemplate
                      releaseId
                    }
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
      id: this.props.releaseId
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

