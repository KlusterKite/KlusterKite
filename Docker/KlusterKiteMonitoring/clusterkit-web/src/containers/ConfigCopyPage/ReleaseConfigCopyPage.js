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

  convertNameToId = (nugetPackages) => {
    // Temporary hack while nugetPackages __id is incorrect
    nugetPackages.edges.forEach((item => {
      item.node.__id = item.node.name;
      delete item.node.name;
    }));

    return nugetPackages;
  };

  copyConfiguration = () => {
    if (this.props.api.klusterKiteNodesApi.releases.edges && this.props.api.klusterKiteNodesApi.releases.edges.length > 0){
      let oldConfiguration = this.props.api.klusterKiteNodesApi.releases.edges[0].node.configuration;

      if (this.props.params.mode === 'update') {
        oldConfiguration.packages = this.convertNameToId(this.props.api.klusterKiteNodesApi.nugetPackages);
      }

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
          if (response.klusterKiteNodeApi_klusterKiteNodesApi_releases_update.errors &&
            response.klusterKiteNodeApi_klusterKiteNodesApi_releases_update.errors.edges) {
            const messages = this.getErrorMessagesFromEdge(response.klusterKiteNodeApi_klusterKiteNodesApi_releases_update.errors.edges);

            this.setState({
              processing: false,
              saveErrors: messages
            });
          } else {
            this.setState({
              processing: false,
              saveErrors: null
            });
            browserHistory.push(`/klusterkite/Release/${this.props.params.releaseId}`);
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
        fragment on IKlusterKiteNodeApi {
          id
          klusterKiteNodesApi {
            releases(filter: { state: Active }, limit: 1) {
              edges {
                node {
                  configuration {
                    nugetFeed
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
                    migratorTemplates {
                      edges {
                        node {
                          id
                          code
                          configuration
                          name
                          notes
                          packageRequirements {
                            edges {
                              node {
                                __id
                                specificVersion
                              }
                            }
                          }
                          packagesToInstall {
                            edges {
                              node {
                                key
                                value {
                                  edges {
                                    node {
                                      __id
                                      version
                                    }
                                  }
                                }
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
            nugetPackages {
              edges {
                node {
                  __id
                  name
                  version
                }
              }
            }
          }
          release: __node(id: $releaseId) @include( if: $nodeExists ) {
            ...on IKlusterKiteNodeApi_Release {
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
