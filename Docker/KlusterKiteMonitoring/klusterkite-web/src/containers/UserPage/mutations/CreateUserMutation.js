import Relay from 'react-relay'

export default class CreateUserMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_users_create}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_User_NodeMutationPayload {
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
          fragment on KlusterKiteNodeApi_User_NodeMutationPayload {
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
        login: this.props.login,
        isBlocked: this.props.isBlocked,
        isDeleted: this.props.isDeleted,
      }
    }
  }

  getOptimisticResponse () {
    return {
      model: {
        login: this.props.login,
        isBlocked: this.props.isBlocked,
        isDeleted: this.props.isDeleted,
      },
    }
  }
}

