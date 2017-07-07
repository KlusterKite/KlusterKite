import React from 'react'
import Relay from 'react-relay'

import delay from 'lodash/delay'

import NodesList from '../../components/NodesList/NodesList';
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
        {hasPrivilege('KlusterKite.NodeManager.GetTemplateStatistics') && this.props.api.klusterKiteNodesApi &&
          <NodesWithTemplates data={this.props.api.klusterKiteNodesApi}/>
        }
        {hasPrivilege('KlusterKite.NodeManager.GetActiveNodeDescriptions') && this.props.api.klusterKiteNodesApi &&
          <NodesList hasError={false} upgradeNodePrivilege={hasPrivilege('KlusterKite.NodeManager.UpgradeNode')}
                     nodeDescriptions={this.props.api.klusterKiteNodesApi}/>
        }
      </div>
    )
  }
}

export default Relay.createContainer(
  HomePage,
  {
    fragments: {
      api: () => Relay.QL`fragment on IKlusterKiteNodeApi {
        __typename
        klusterKiteNodesApi {
          id
          ${NodesWithTemplates.getFragment('data')},
          ${NodesList.getFragment('nodeDescriptions')},
        }
      }
      `,
    }
  },
)
