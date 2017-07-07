import Relay from 'react-relay'

export default class CloneConfigMutation extends Relay.Mutation {
  getMutation () {
    return Relay.QL`mutation{klusterKiteNodeApi_klusterKiteNodesApi_releases_update}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on KlusterKiteNodeApi_Release_NodeMutationPayload {
        node
        edge
        errors {
          edges {
            node {
              field
              message
            }
          }
        }
        api {
          klusterKiteNodesApi {
            releases
          }
        }
      }
    `
  }

  getConfigs () {
    return [{
      type: 'REQUIRED_CHILDREN',
      children: [
        Relay.QL`
          fragment on KlusterKiteNodeApi_Release_NodeMutationPayload {
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
    }]
  }

  /**
   * Convert edges list to an array of nodes; cleans unnecessary properties
   * @param edges {Object} Edges list
   * @param type {string} Converted object type
   * @returns {Object[]} Array of nodes
   */
  convertEdgesToArray(edges, type){
    const oldNodes = edges.map(x => x.node);

    let nodes = [];
    oldNodes.forEach(node => {
      const keys = Object.keys(node);
      let newNode = {};
      keys.forEach(key => {
        if (key !== '__id' && key !== '__dataID__' && key !== 'id' && key !== 'packagesToInstall'){
          if (typeof(node[key]) === 'object' && node[key] && node[key].edges) {
            newNode[key] = this.convertEdgesToArray(node[key].edges, key);
          } else {
            newNode[key] = node[key];
          }
        }

        if (type === 'packages' && key === '__id') {
          newNode['id'] = node[key];
        }

        if (type === 'packageRequirements' && key === '__id') {
          newNode['id'] = node[key];
        }
      });

      if (newNode) {
        nodes.push(newNode);
      }
    });

    if (type === 'migrationTemplates') {
      console.log('migrator templates', nodes);
    }

    return nodes;
  }

  getVariables () {
    return {
      id: this.props.releaseId,
      newNode: {
        id: this.props.releaseId,
        configuration: {
          migratorTemplates: this.convertEdgesToArray(this.props.configuration.migratorTemplates.edges, 'migrationTemplates'),
          nodeTemplates: this.convertEdgesToArray(this.props.configuration.nodeTemplates.edges, 'nodeTemplates'),
          nugetFeed: this.props.configuration.nugetFeed,
          packages: this.convertEdgesToArray(this.props.configuration.packages.edges, 'packages'),
          seedAddresses: this.props.configuration.seedAddresses
        },
      }
    }
  }

  getOptimisticResponse () {
    return {
      model: {
        id: this.props.nodeId,
        configuration: {
          migratorTemplates: this.props.configuration.migratorTemplates,
          nodeTemplates: this.props.configuration.nodeTemplates,
          nugetFeed: this.props.configuration.nugetFeed,
          packages: this.props.configuration.packages,
          seedAddresses: this.props.configuration.seedAddresses,
          id: this.props.configuration.id
        },
      },
    }
  }
}

