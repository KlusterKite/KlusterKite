import Relay from 'react-relay'

export default class UpdateResourcesMutation extends Relay.Mutation {

  getMutation () {
    return Relay.QL`mutation{clusterKitNodeApi_clusterKitNodesApi_clusterManagement_migrationResourceUpdate}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on Boolean_MutationPayload {
        result
      }
    `
  }

  getConfigs() {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
          Relay.QL`
          fragment on Boolean_MutationPayload {
            result
          }
        `,
      ],
    }];
  }

  getVariables () {
    return {
      request: {
        resources: [
          {
            templateCode: this.props.templateCode,
            migratorTypeName: this.props.migratorTypeName,
            resourceCode: this.props.resourceCode,
            target: this.props.target
          }
        ]
      }
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

