import React from 'react'
import Relay from 'react-relay'

import ReloadPackages from '../../components/ReloadPackages/index';
import NodesList from '../../components/NodesList/index';
import NodesWithTemplates from '../../components/NodesWithTemplates/index';

class HomePage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  onNodeUpgrade = () => {
    console.log('upgrade');
    return false;
  };

  refetchData = () => {
    console.log('refetch');
    this.props.relay.forceFetch();
  };

  render () {
    return (
      <div>
        <h1>Monitoring</h1>
        {false && <button type="button" className="btn btn-primary btn-lg" onClick={this.refetchData}>
          <i className="fa fa-refresh"/> {' '} Refetch
        </button>}
        <ReloadPackages />
        {this.props.api.nodeManagerData &&
          <NodesWithTemplates data={this.props.api.nodeManagerData}/>
        }
        {this.props.api.nodeManagerData &&
          <NodesList hasError={false} upgradeNodePrivilege={true} onManualUpgrade={this.onNodeUpgrade}
                     nodeDescriptions={this.props.api.nodeManagerData}/>
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  HomePage,
  {
    fragments: {
      api: () => Relay.QL`fragment on ClusterKitNodeApi_ClusterKitNodeApi {
        nodeManagerData {
          ${NodesWithTemplates.getFragment('data')},
          ${NodesList.getFragment('nodeDescriptions')},
        }
      }
      `,
    }
  },
)
