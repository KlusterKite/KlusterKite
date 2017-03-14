import React from 'react'
import Relay from 'react-relay'

import TemplatesList from '../../components/TemplatesList/index';

class TemplatesListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    return (
      <div>
        <TemplatesList templates={this.props.api.nodeManagerData} createNodeTemplatePrivilege={true} getNodeTemplatePrivilege={true} />
      </div>
    )
  }
}

export default Relay.createContainer(
  TemplatesListPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on ClusterKitNodeApi_ClusterKitNodeApi {
        nodeManagerData {
          ${TemplatesList.getFragment('templates')},
        }
      }
      `,
    }
  },
)
