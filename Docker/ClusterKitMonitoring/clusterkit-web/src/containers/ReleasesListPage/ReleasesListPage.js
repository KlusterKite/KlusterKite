import React from 'react'
import Relay from 'react-relay'

import ReleasesList from '../../components/ReleasesList/ReleasesList';

class ReleasesListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
    page: React.PropTypes.string,
  };

  render () {
    const page = this.props.page ? Number.parseInt(this.props.page, 10) : 1;
    const itemsPerPage = 10;
    const offset = (page - 1) * itemsPerPage;

    return (
      <div>
        <ReleasesList
          clusterKitNodesApi={this.props.api.clusterKitNodesApi}
          loaded={false}
          itemsPerPage={itemsPerPage}
          offset={offset}
          currentPage={page}
        />
      </div>
    )
  }
}

// ${FeedsListOld.getFragment('feeds')},
export default Relay.createContainer(
  ReleasesListPage,
  {
    initialVariables: {
      itemsPerPage: 10,
      page: null,
      offset: null,
      loaded: false,
    },
    prepareVariables: (variables) => {
      if (variables.page && variables.itemsPerPage)
        return { ...variables, offset: (variables.page-1) * variables.itemsPerPage, loaded: true };
      else if (variables.itemsPerPage)
        return { ...variables, offset: 0, loaded: true };
      else
        return variables;
    },
    fragments: {
      api: (variables) => Relay.QL`fragment on IClusterKitNodeApi {
        __typename
        clusterKitNodesApi {
          id
          ${ReleasesList.getFragment('clusterKitNodesApi', { page: variables.page, itemsPerPage: variables.itemsPerPage, offset: variables.offset})},
        }
      }
      `,
    }
  },
)
