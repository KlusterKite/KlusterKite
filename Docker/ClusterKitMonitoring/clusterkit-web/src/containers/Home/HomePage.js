import React from 'react'
import Relay from 'react-relay'

import delay from 'lodash/delay'

import ReloadPackages from '../../components/ReloadPackages/index';
import NodesList from '../../components/NodesList/index';
import NodesWithTemplates from '../../components/NodesWithTemplates/index';

import { hasPrivilege } from '../../utils/privileges';

class HomePage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  componentDidMount = () => {
    delay(() => this.refetchDataOnTimer(), 10000);
  };

  componentWillUnmount = () => {
    clearTimeout(this._refreshId);
  };

  refetchDataOnTimer = () => {
    this.props.relay.forceFetch();
    this._refreshId = delay(() => this.refetchDataOnTimer(), 10000);
  };

  render () {
    return (
      <div>
        <h1>Monitoring</h1>
        {hasPrivilege('ClusterKit.NodeManager.ReloadPackages') &&
          <ReloadPackages />
        }
        {hasPrivilege('ClusterKit.NodeManager.GetTemplateStatistics') && this.props.api.nodeManagerData &&
          <NodesWithTemplates data={this.props.api.nodeManagerData}/>
        }
        {hasPrivilege('ClusterKit.NodeManager.GetActiveNodeDescriptions') && this.props.api.nodeManagerData &&
          <NodesList hasError={false} upgradeNodePrivilege={hasPrivilege('ClusterKit.NodeManager.UpgradeNode')}
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
      api: () => Relay.QL`fragment on IClusterKitNodeApi {
        __typename
        nodeManagerData {
          ${NodesWithTemplates.getFragment('data')},
          ${NodesList.getFragment('nodeDescriptions')},
        }
      }
      `,
    }
  },
)
