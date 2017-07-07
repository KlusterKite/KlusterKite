import Relay from 'react-relay'

export default class UpdateUserMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{clusterKitNodeApi_clusterKitNodesApi_users_update}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_User_NodeMutationPayload {
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
          fragment on ClusterKitNodeApi_User_NodeMutationPayload {
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
        login: this.props.login
      }
    }
  }

  getOptimisticResponse () {
    return {
      model: {
        id: this.props.nodeId,
        uid: this.props.uid,
        login: this.props.login
      },
    }
  }
}

