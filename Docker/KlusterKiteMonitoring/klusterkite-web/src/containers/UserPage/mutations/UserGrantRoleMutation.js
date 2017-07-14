import Relay from 'react-relay'

export default class UserGrantRoleMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_users_grantRole}`
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
      userUid: this.props.userUid,
      roleUid: this.props.roleUid,
    }
  }

  getOptimisticResponse () {
    return {
      userUid: this.props.userUid,
      roleUid: this.props.roleUid,
    }
  }
}

