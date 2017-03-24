import React from 'react'
import Relay from 'react-relay'

import delay from 'lodash/delay'

import ActorsTreeButtons from '../../components/ActorsTree/buttons';
import ActorsTree from '../../components/ActorsTree/tree';
import InitiateScanMutation from './mutations/InitiateScanMutation'

class ActorsTreePage extends React.Component {

  static propTypes = {
    api: React.PropTypes.object
  }

  constructor (props) {
    super(props)
    this.state = {
      isScanning: false,
    }
  }

  onInitiateScan = () => {
    console.log('initiating!');

    Relay.Store.commitUpdate(
      new InitiateScanMutation({}),
      {
        onSuccess: (response) => {
          const result = response.clusterKitMonitoring_clusterKitMonitoringApi_initiateScan.result;

          console.log('response', response);
          if (result) {
            console.log('success');
          }

          this.setScanning(true);
          this.stopScanningAfterDelay();
        },
        onFailure: (transaction) => console.log(transaction),
      },
    )
  };

  /**
   * Sets scanning flag
   * @param value {Boolean} New state
   */
  setScanning = (value) => {
    this.setState({
      isScanning: value
    });
  };

  /**
   * Removes scanning flag after delay
   */
  stopScanningAfterDelay = () => {
    delay(() => this.setScanning(false), 5000);
  };

  onReload = () => {
    console.log('reloading');
  };

  render () {
    return (
      <div>
        <ActorsTreeButtons handleScan={this.onInitiateScan} handleReload={this.onReload} isLoading={this.state.isScanning} />
        {this.props.api.clusterKitMonitoringApi && this.props.api.clusterKitMonitoringApi.getClusterTree &&  this.props.api.clusterKitMonitoringApi.getClusterTree.nodes &&
          <ActorsTree tree={this.props.api.clusterKitMonitoringApi.getClusterTree.nodes.edges} />
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  ActorsTreePage,
  {
    fragments: {
      api: () => Relay.QL`
        fragment on ClusterKitMonitoring_ClusterKitNodeApi {
          __typename
          clusterKitMonitoringApi {
            getClusterTree {
              nodes {
                edges {
                  node {
                    value {
                      name
                      actorType
                      dispatcherType
                      currentMessage
                      queueSize
                      queueSizeSum
                      maxQueueSize
                      children {
                        edges {
                          node {
                            name
                            actorType
                            dispatcherType
                            currentMessage
                            queueSize
                            queueSizeSum
                            maxQueueSize
                            children {
                              edges {
                                node {
                                  name
                                  actorType
                                  dispatcherType
                                  currentMessage
                                  queueSize
                                  queueSizeSum
                                  maxQueueSize
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      `,
    },
  },
)
