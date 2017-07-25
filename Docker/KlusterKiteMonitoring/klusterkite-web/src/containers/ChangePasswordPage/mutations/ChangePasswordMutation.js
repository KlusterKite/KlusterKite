import Relay from 'react-relay'

export default class ChangePasswordMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_me_changePassword}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_MutationResult_System_Boolean__MutationPayload {
        result
        {
          result
          errors
        }
      }
    `
  }

  getConfigs() {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
        Relay.QL`
          fragment on KlusterKiteNodeApi_MutationResult_System_Boolean__MutationPayload {
            result
            {
              result
              errors
              {
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
      oldPassword: this.props.oldPassword,
      newPassword: this.props.newPassword,
    }
  }

  getOptimisticResponse () {
    return {
      oldPassword: this.props.oldPassword,
      newPassword: this.props.newPassword,
    }
  }
}

