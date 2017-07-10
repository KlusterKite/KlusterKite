import React from 'react';
import Relay from 'react-relay'
import { browserHistory } from 'react-router'

import { Link } from 'react-router';

import Paginator from '../Paginator/Paginator';
import DateFormat from '../../utils/date';

export class ConfigurationsList extends React.Component {
  //
  // constructor(props) {
  //   super(props);
  // }

  static propTypes = {
    klusterKiteNodesApi: React.PropTypes.object,
    itemsPerPage: React.PropTypes.number,
    currentPage: React.PropTypes.number,
  };

  onPageChange(page) {
    browserHistory.push(`/klusterkite/Configurations/${page}`);
  }

  render() {
    if (!this.props.klusterKiteNodesApi.configurations){
      return (<div></div>);
    }
    const edges = this.props.klusterKiteNodesApi.configurations.edges;

    return (
      <div>
        <h3>Configurations list</h3>
        <Link to={`/klusterkite/Configuration/create`} className="btn btn-primary" role="button">Add a new configuration</Link>
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Name</th>
              <th>Created</th>
              <th>Finished</th>
              <th>State</th>
              <th>Stable?</th>
            </tr>
          </thead>
          <tbody>
          {edges && edges.map((edge) => {
            const node = edge.node;
            const dateCreated = new Date(node.created);
            const dateFinished = node.finished ? new Date(node.finished) : null;

            return (
              <tr key={`${node.id}`}>
                <td>
                  <Link to={`/klusterkite/Configuration/${encodeURIComponent(node.id)}`}>
                    {node.name}
                  </Link>
                </td>
                <td>{DateFormat.formatDateTime(dateCreated)}</td>
                <td>{dateFinished && DateFormat.formatDateTime(dateFinished)}</td>
                <td>{node.state}</td>
                <td>{node.isStable.toString()}</td>
              </tr>
            )
          })
          }
          </tbody>
        </table>
        <Paginator
          totalItems={this.props.klusterKiteNodesApi.configurations.count}
          currentPage={this.props.currentPage}
          itemsPerPage={this.props.itemsPerPage}
          onSelect={this.onPageChange}
        />

      </div>
    );
  }
}

export default Relay.createContainer(
  ConfigurationsList,
  {
    initialVariables: {
      itemsPerPage: null,
      offset: null,
    },
    fragments: {
      klusterKiteNodesApi: (variables) => Relay.QL`fragment on IKlusterKiteNodeApi_Root {
        configurations(sort: created_desc, limit: $itemsPerPage, offset: $offset) {
          edges {
            node {
              id
              name
              notes
              minorVersion
              majorVersion
              created
              started
              finished
              state
              isStable
            }
          }
          count
        }
      }
      `,
    },
  },
)
