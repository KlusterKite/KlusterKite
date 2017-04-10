import Relay from 'react-relay'

export default class UpdateFeedMutation extends Relay.Mutation {
  static fragments = {
    configuration: () => Relay.QL`
      fragment on IClusterKitNodeApi_ReleaseConfiguration {
        nodeTemplates {
          edges {
            node {
              id
              code
              configuration
              containerTypes
              minimumRequiredInstances
              maximumNeededInstances
              name
              packageRequirements {
                edges {
                  node {
                    __id
                    specificVersion
                  }
                }
              }
              priority
            }
          }
        }
        nugetFeeds {
          edges {
            node {
              id
              address
              type
              userName
              password
            }
          }
        }
        packages {
          edges {
            node {
              version
              id
              __id
            }
          }
        }
        seedAddresses
        id
      }
    `,
  };

  getMutation () {
    return Relay.QL`mutation{ClusterKitNodeApi_clusterKitNodesApi_releases_update}`
  }

  getFatQuery () {
    return Relay.QL`
      fragment on ClusterKitNodeApi_Release_NodeMutationPayload {
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
          clusterKitNodesApi {
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
    const typeSingular = type.substring(0, type.length - 1);

    let nodes = [];
    oldNodes.forEach(node => {
      const keys = Object.keys(node);
      let newNode = {};
      keys.forEach(key => {
        if (key !== '__id' && key !== '__dataID__' && key !== 'id'){
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

      // Updating a record
      if (this.props[typeSingular] && this.props[`${typeSingular}Id`] === node.id) {
        newNode = this.props[typeSingular];
      }

      // Delete a record
      if (this.props[typeSingular] && this.props[`${typeSingular}DeleteId`] === node.id) {
        newNode = null
      }

      if (newNode) {
        nodes.push(newNode);
      }
    });

    // Adding a record
    if (this.props[typeSingular]) {
      console.log('Creating or updating', this.props[`${typeSingular}Id`]);
    }
    if (this.props[typeSingular] && this.props[`${typeSingular}Id`] === null) {
      nodes.push(this.props[typeSingular]);
    }

    return nodes;
  }

  getVariables () {
    return {
      id: this.props.releaseId,
      newNode: {
        id: this.props.releaseId,
        configuration: {
          nodeTemplates: this.convertEdgesToArray(this.props.configuration.nodeTemplates.edges, 'nodeTemplates'),
          nugetFeeds: this.convertEdgesToArray(this.props.configuration.nugetFeeds.edges, 'nugetFeeds'),
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
          nodeTemplates: this.props.configuration.nodeTemplates,
          nugetFeeds: this.props.configuration.nugetFeeds,
          packages: this.props.configuration.packages,
          seedAddresses: this.props.configuration.seedAddresses,
          id: this.props.configuration.id
        },
      },
    }
  }
}

