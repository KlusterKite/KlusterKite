import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

class NodeTemplatesList extends React.Component {
  constructor (props) {
    super(props);
    this.state = {
    }
  }

  static propTypes = {
    releaseId: React.PropTypes.string,
    configuration: React.PropTypes.object,
    createNodeTemplatePrivilege: React.PropTypes.bool.isRequired,
    getNodeTemplatePrivilege: React.PropTypes.bool.isRequired,
    canEdit: React.PropTypes.bool
  };

  render() {
    const templates = this.props.configuration && this.props.configuration.nodeTemplates && this.props.configuration.nodeTemplates.edges;

    return (
      <div>
        <h3>Node templates list</h3>
        {this.props.canEdit &&
          <Link to={`/clusterkit/NodeTemplates/${this.props.releaseId}/create`} className="btn btn-primary" role="button">Add a new template</Link>
        }
        {templates && templates.length > 0 &&
        <table className="table table-hover">
          <thead>
          <tr>
            <th>Code</th>
            <th>Name</th>
            <th>Packages</th>
            <th>Min</th>
            <th>Max</th>
            <th>Priority</th>
          </tr>
          </thead>
          <tbody>
          {templates.map((item) =>
            <tr key={item.node.id}>
              <td>
                {this.props.canEdit &&
                <Link to={`/clusterkit/NodeTemplates/${this.props.releaseId}/${encodeURIComponent(item.node.id)}`}>
                  {item.node.code}
                </Link>
                }
                {!this.props.canEdit &&
                <span>{item.node.code}</span>
                }
              </td>
              <td>{item.node.name}</td>
              <td>
                {item.node.packageRequirements.edges.map((pack) =>
                  <span key={`${item.Id}/${pack.node.__id}`}>
                    <span className="label label-default">{pack.node.__id}</span>{' '}
                  </span>
                )
                }
              </td>
              <td>{item.node.minimumRequiredInstances}</td>
              <td>{item.node.maximumNeededInstances}</td>
              <td>{item.node.priority}</td>
            </tr>
          )
          }
          </tbody>
        </table>
        }
      </div>
    );
  }
}

export default Relay.createContainer(
  NodeTemplatesList,
  {
    fragments: {
      configuration: () => Relay.QL`fragment on IClusterKitNodeApi_ReleaseConfiguration {
        nodeTemplates {
          edges {
            node {
              id
              code
              configuration
              containerTypes
              minimumRequiredInstances
              maximumNeededInstances
              name
              packageRequirements {
                edges {
                  node {
                    __id
                    specificVersion
                  }
                }
              }
              priority
            }
          }
        }
      }
      `,
    },
  },
)
