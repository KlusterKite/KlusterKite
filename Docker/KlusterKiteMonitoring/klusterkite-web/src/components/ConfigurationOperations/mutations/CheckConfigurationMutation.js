import Relay from 'react-relay'

export default class CheckConfigurationMutation extends Relay.Mutation {

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
                    configurationId
                    compatibleConfigurationId
                  }
                }
              }
              compatibleTemplatesBackward {
                edges {
                  node {
                    templateCode
                    configurationId
                    compatibleConfigurationId
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
                      configurationId
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
      id: this.props.configurationId
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

