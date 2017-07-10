import Relay from 'react-relay'

export default class CreateRoleMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_roles_create}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_Role_NodeMutationPayload {
        node
        edge
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

  // getConfigs () {
  //   return [{
  //     type: 'FIELDS_CHANGE',
  //     fieldIDs: {
  //       node: this.props.nodeId,
  //     },
  //   }]
  // }

  getVariables () {
    return {
      id: this.props.__id,
      newNode: {
        id: this.props.__id,
        name: this.props.name,
        allowedScope: this.props.allowedScope,
        deniedScope: this.props.deniedScope,
      }
    }
  }

  getOptimisticResponse () {
    return {
      edge: {
        node: {
          id: this.props.nodeId,
          name: this.props.name,
          allowedScope: this.props.allowedScope,
          deniedScope: this.props.deniedScope,
        },
      },
    }
  }
}

