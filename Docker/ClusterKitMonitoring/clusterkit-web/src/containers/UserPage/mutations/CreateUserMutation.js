import Relay from 'react-relay'

export default class CreateUserMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_clusterKitNodesApi_users_create}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_User_NodeMutationPayload {
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
      id: this.props.__id,
      newNode: {
        id: this.props.__id,
        majorVersion: this.props.majorVersion,
        minorVersion: this.props.minorVersion,
        name: this.props.name,
        notes: this.props.notes
      }
    }
  }

  getOptimisticResponse () {
    return {
      edge: {
        node: {
          id: this.props.nodeId,
          majorVersion: this.props.majorVersion,
          minorVersion: this.props.minorVersion,
          name: this.props.name,
          notes: this.props.notes
        },
      },
    }
  }
}

