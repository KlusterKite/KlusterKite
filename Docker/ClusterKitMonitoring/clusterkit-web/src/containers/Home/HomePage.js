import React from 'react'
import Relay from 'react-relay'

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

  // <NodesList hasError={false} upgradeNodePrivilege={true} onManualUpgrade={this.onNodeUpgrade} nodeDescriptions={this.props.api.nodeManagerData} />
  // <NodesWithTemplates data={this.props.api.nodeManagerData} />
  render () {
    return (
      <div>
        <NodesWithTemplates data={this.props.api.nodeManagerData} />
        <NodesList hasError={false} upgradeNodePrivilege={true} onManualUpgrade={this.onNodeUpgrade} nodeDescriptions={this.props.api.nodeManagerData} />
      </div>
    )
  }
}

export default Relay.createContainer(
  HomePage,
  {
    // ${NodesList.getFragment('nodeDescriptions')},
    // ${NodesWithTemplates.getFragment('data')},
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
