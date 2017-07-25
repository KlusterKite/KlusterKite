import React from 'react'
import Relay from 'react-relay'

import delay from 'lodash/delay'

import ActorsTreeButtons from '../../components/ActorsTree/ActorsTreeButtons';
import ActorsTreeFlat from '../../components/ActorsTree/ActorsTreeFlat';
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
          // const result = response.klusterKiteMonitoring_klusterKiteMonitoringApi_initiateScan.result;
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
    delay(() => this.stopScanning(), 5000);
  };

  stopScanning = () => {
    this.setScanning(false);
    this.props.relay.forceFetch();
  };

  onReload = () => {
    console.log('reloading');
    this.props.relay.forceFetch();
  };

  render () {
    return (
      <div>
        <ActorsTreeButtons handleScan={this.onInitiateScan} handleReload={this.onReload} isLoading={this.state.isScanning} />
        {this.props.api.klusterKiteMonitoringApi && this.props.api.klusterKiteMonitoringApi.getClusterTree &&  this.props.api.klusterKiteMonitoringApi.getClusterTree.nodesFlat &&
          <div>
            <ActorsTreeFlat
              tree={this.props.api.klusterKiteMonitoringApi.getClusterTree.nodesFlat.edges}
            />
          </div>
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
        fragment on IKlusterKiteMonitoring {
          __typename
          klusterKiteMonitoringApi {
            getClusterTree {
              nodesFlat {
                edges {
                  node {
                    name
                    actorType
                    dispatcherType
                    currentMessage
                    queueSize
                    queueSizeSum
                    maxQueueSize
                    address
                    parentAddress
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
