import Relay from 'react-relay'

export default class UpdateUserMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_users_update}`
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
      id: this.props.uid,
      newNode: {
        uid: this.props.uid,
        login: this.props.login,
        isBlocked: this.props.isBlocked,
        isDeleted: this.props.isDeleted,
      }
    }
  }

  getOptimisticResponse () {
    return {
      model: {
        id: this.props.nodeId,
        uid: this.props.uid,
        login: this.props.login,
        isBlocked: this.props.isBlocked,
        isDeleted: this.props.isDeleted,
      },
    }
  }
}

