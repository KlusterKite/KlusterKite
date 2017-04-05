import React from 'react';

import TemplateForm from '../TemplateForm/index';
import { Link } from 'react-router';

class TemplatesList extends React.Component {
  constructor (props) {
    super(props);
    this.state = {
      isEditing: false,
    }
  }

  static propTypes = {
    nodeTemplates: React.PropTypes.object,
    onChange: React.PropTypes.func,
    createNodeTemplatePrivilege: React.PropTypes.bool.isRequired,
    getNodeTemplatePrivilege: React.PropTypes.bool.isRequired,
  };

  /**
   * Shows edit form for the selected feed
   * @param node {Object} Feed data or null (if new)
   */
  showEditForm(node) {
    this.setState({
      isEditing: true,
      editedObject: node
    });
  };

  /**
   * Creates or updates a feed record. Pushes data to the this.props.onChange method.
   * @param model {Object} New feed model
   */
  createOrUpdate(model) {
    console.log('createOrUpdate', model);

    this.setState({
      isEditing: false,
      editedObject: null
    });
    // this.props.onChange(newFeeds);
  }

  /**
   * Hides an edit form
   */
  onCancel() {
    console.log('cancel');

    this.setState({
      isEditing: false,
      editedObject: null
    });
  }

  render() {
    console.log('templates', this.props.nodeTemplates);
    const templates = this.props.nodeTemplates && this.props.nodeTemplates.edges;

    return (
      <div>
        {!this.state.isEditing &&
        <div>
          <h2>Templates list</h2>
          {this.props.createNodeTemplatePrivilege &&
            <a onClick={() => this.showEditForm(null)} className="btn btn-primary">Add a new template</a>
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
            </tr>
            </thead>
            <tbody>
            {templates && templates.length > 0 && templates.map((item) =>
              <tr key={item.node.id}>
                <td>
                  {this.props.getNodeTemplatePrivilege &&
                    <Link to={`/clusterkit/Templates/${this.props.releaseId}/${encodeURIComponent(item.node.id)}`}>
                      {item.node.code}
                    </Link>
                  }
                  {false && this.props.getNodeTemplatePrivilege &&
                    <a onClick={() => this.showEditForm(item.node)} className="pointer">
                      {item.node.code}
                    </a>
                  }
                  {!this.props.getNodeTemplatePrivilege &&
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
        </div>
        }
        {this.state.isEditing &&
          <div>
            <TemplateForm initialValues={this.state.editedObject} onSubmit={(model) => this.createOrUpdate(model)} onCancel={this.onCancel} />
          </div>
        }
      </div>
    );
  }
}

export default TemplatesList
