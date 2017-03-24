import React from 'react'
import Relay from 'react-relay'

import { hasPrivilege } from '../../utils/privileges';
import TemplatesList from '../../components/TemplatesList/index';

class TemplatesListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    return (
      <div>
        <TemplatesList
          templates={this.props.api.nodeManagerData}
          createNodeTemplatePrivilege={hasPrivilege('ClusterKit.NodeManager.NodeTemplate.Create')}
          getNodeTemplatePrivilege={hasPrivilege('ClusterKit.NodeManager.NodeTemplate.Get')} />
      </div>
    )
  }
}

export default Relay.createContainer(
  TemplatesListPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on ClusterKitMonitoring_ClusterKitNodeApi {
        __typename
        nodeManagerData {
          ${TemplatesList.getFragment('templates')},
        }
      }
      `,
    }
  },
)
