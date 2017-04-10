import React from 'react'
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import CloneConfigMutation from './mutations/CloneConfigMutation'

class ReleaseConfigCopyPage extends React.Component {

  static propTypes = {
    api: React.PropTypes.object,
    params: React.PropTypes.object,
  };

  constructor (props) {
    super(props);
    this.state = {
      processing: false,
      oldConifgNotFoundError: false
    }
  }

  componentWillMount() {
    this.copyConfiguration();
  }

  copyConfiguration = () => {
    if (this.props.api.clusterKitNodesApi.releases.edges && this.props.api.clusterKitNodesApi.releases.edges.length > 0){
      const oldConfiguration = this.props.api.clusterKitNodesApi.releases.edges[0].node.configuration;

      this.setState({
        processing: true
      });
      this.cloneConfig(oldConfiguration);

    } else {
      this.setState({
        oldConifgNotFoundError: true
      });
    }
  };

  cloneConfig = (config) => {
    Relay.Store.commitUpdate(
      new CloneConfigMutation(
        {
          nodeId: this.props.params.releaseId,
          releaseId: this.props.api.release.__id,
          configuration: config,
        }),
      {
        onSuccess: (response) => {
          console.log('response', response);
          if (response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors &&
            response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.clusterKitNodeApi_clusterKitNodesApi_releases_update.errors.edges);

            this.setState({
              processing: false,
              saveErrors: messages
            });
          } else {
            this.setState({
              processing: false,
              saveErrors: null
            });
            browserHistory.push(`/clusterkit/Releases/${this.props.params.releaseId}`);
          }
        },
        onFailure: (transaction) => {
          this.setState({
            processing: false
          });
          console.log(transaction)},
      },
    )
  };

  getErrorMessagesFromEdge = (edges) => {
    return edges.map(x => x.node).map(x => x.message);
  };

  render () {
    return (
      <div>
        {this.state.oldConifgNotFoundError &&
          <div>
            <h2>Error!</h2>
            <p>Old configuration is not found.</p>
          </div>
        }
        {this.state.processing &&
          <div>
            <h2>Copying config</h2>
            <p>Please wait…</p>
          </div>
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  ReleaseConfigCopyPage,
  {
    initialVariables: {
      releaseId: null,
      nodeExists: false,
    },
    prepareVariables: (prevVariables) => Object.assign({}, prevVariables, {
      nodeExists: prevVariables.releaseId !== null,
    }),
    fragments: {
      api: () => Relay.QL`
        fragment on IClusterKitNodeApi {
          id
          clusterKitNodesApi {
            releases(filter: { state: Active }, limit: 1) {
              edges {
                node {
                  configuration {
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
                }
              }
            }
          }
          release: __node(id: $releaseId) @include( if: $nodeExists ) {
            ...on IClusterKitNodeApi_Release {
              __id
              name
              notes
              minorVersion
              majorVersion
              state
            }
          }
        }
      `,
    },
  },
)
