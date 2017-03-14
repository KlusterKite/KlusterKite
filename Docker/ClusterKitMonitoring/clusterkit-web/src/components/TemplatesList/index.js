import React from 'react';
import Relay from 'react-relay'

import { Link } from 'react-router';

class TemplatesList extends React.Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    templates: React.PropTypes.object,
    createNodeTemplatePrivilege: React.PropTypes.bool.isRequired,
    getNodeTemplatePrivilege: React.PropTypes.bool.isRequired,
  };

  render() {
    const templates = this.props.templates.nodeTemplates.edges;
    return (
      <div>
        <h2>Templates list</h2>
        {this.props.createNodeTemplatePrivilege &&
          <Link to="/Templates/create/" className="btn btn-primary" role="button">Add a new template</Link>
        }
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Code</th>
              <th>Name</th>
              <th>Packages</th>
              <th>Min</th>
              <th>Max</th>
              <th>Priority</th>
              <th>Version</th>
            </tr>
          </thead>
          <tbody>
          {templates && templates.length > 0 && templates.map((item) =>
            <tr key={item.node.id}>
              <td>
                {this.props.getNodeTemplatePrivilege &&
                  <Link to={`/Templates/${encodeURIComponent(item.node.id)}`}>
                    {item.node.code}
                  </Link>
                }
                {!this.props.getNodeTemplatePrivilege &&
                  <span>{item.node.code}</span>
                }
              </td>
              <td>{item.node.name}</td>
              <td>
                {item.node.packages.map((pack) =>
                  <span key={`${item.Id}/${pack}`}>
                    <span className="label label-default">{pack}</span>{' '}
                  </span>
                )
                }
              </td>
              <td>{item.node.minimumRequiredInstances}</td>
              <td>{item.node.maximumNeededInstances}</td>
              <td>{item.node.priority}</td>
              <td>{item.node.version}</td>
            </tr>
          )
          }
          </tbody>
        </table>
      </div>
    );
  }
}

export default Relay.createContainer(
  TemplatesList,
  {
    fragments: {
      templates: () => Relay.QL`fragment on ClusterKitNodeApi_ClusterKitNodeManagement {
        nodeTemplates {
          edges {
            node {
              id
              code
              minimumRequiredInstances
              maximumNeededInstances
              name
              packages
              priority
              version
            }
          }
        }
      }
      `,
    },
  },
)
