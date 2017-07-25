import Relay from 'react-relay'

export default class CreateRoleMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_roles_create}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_Role_NodeMutationPayload {
        node
        errors
      }
    `
  }

  getConfigs() {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
        Relay.QL`
          fragment on KlusterKiteNodeApi_Role_NodeMutationPayload {
            errors {
              edges {
                node {
                  field
                  message
                }
              }
            }
            node
          }
        `,
      ],
    }];
  }

  getVariables () {
    return {
      newNode: {
        name: this.props.name,
        allowedScope: this.props.allowedScope,
        deniedScope: this.props.deniedScope,
      }
    }
  }

  getOptimisticResponse () {
    return {
      model: {
        name: this.props.name,
        allowedScope: this.props.allowedScope,
        deniedScope: this.props.deniedScope,
      },
    }
  }
}

