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
    canEdit: React.PropTypes.bool
  };

  render() {
    const templates = this.props.configuration && this.props.configuration.migratorTemplates && this.props.configuration.migratorTemplates.edges;

    return (
      <div>
        <h3>Migrator templates list</h3>
        {this.props.canEdit &&
          <Link to={`/klusterkite/MigratorTemplates/${this.props.releaseId}/create`} className="btn btn-primary" role="button">Add a new template</Link>
        }
        {templates && templates.length > 0 &&
        <table className="table table-hover">
          <thead>
          <tr>
            <th>Code</th>
            <th>Name</th>
            <th>Packages</th>
            <th>Priority</th>
          </tr>
          </thead>
          <tbody>
          {templates.map((item) =>
            <tr key={item.node.id}>
              <td>
                {this.props.canEdit &&
                <Link to={`/klusterkite/MigratorTemplates/${this.props.releaseId}/${encodeURIComponent(item.node.id)}`}>
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
      configuration: () => Relay.QL`fragment on IKlusterKiteNodeApi_ReleaseConfiguration {
        migratorTemplates {
          edges {
            node {
              id
              code
              configuration
              name
              notes
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

/*
packagesToInstall {
  edges {
    node {
      key
      value {
        edges {
          node {
            __id
            version
          }
        }
      }
    }
  }
}
*/
