import Relay from 'react-relay'

export default class UpdateReleaseMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{clusterKitNodeApi_clusterKitNodesApi_releases_update}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_Release_NodeMutationPayload {
        node
        edge
        errors
        api {
          clusterKitNodesApi {
            releases
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
          fragment on ClusterKitNodeApi_Release_NodeMutationPayload {
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
      model: {
        id: this.props.nodeId,
        majorVersion: this.props.majorVersion,
        minorVersion: this.props.minorVersion,
        name: this.props.name,
        notes: this.props.notes
      },
    }
  }
}

