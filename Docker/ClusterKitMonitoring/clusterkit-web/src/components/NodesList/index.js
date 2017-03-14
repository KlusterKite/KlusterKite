import React from 'react';
import Relay from 'react-relay'

import { Popover, OverlayTrigger, Button } from 'react-bootstrap';
import Icon from 'react-fa';

import './styles.css';

export class NodesList extends React.Component {

  constructor(props) {
    super(props);
    this.nodePopover = this.nodePopover.bind(this);
  }

  static propTypes = {
    nodeDescriptions: React.PropTypes.object,
    onManualUpgrade: React.PropTypes.func.isRequired,
    hasError: React.PropTypes.bool.isRequired,
    upgradeNodePrivilege: React.PropTypes.bool.isRequired,
  };

  drawRole(node, role) {
    const isLeader = node.leaderInRoles.indexOf(role) >= 0;
    return (<span key={`${node.NodeId}/${role}`}>
                {isLeader && <span className="label label-info" title={`${role} leader`}>{role}</span>}
                {!isLeader && <span className="label label-default">{role}</span>}
                {' '}
    </span>);
  }

  nodePopover(node) {
    return (
      <Popover title={`${node.nodeTemplate}`} id={`${node.nodeId}`}>
        {node.modules.map((subModule) =>
          <span key={`${node.nodeId}/${subModule.id}`}>
            <span className="label label-default">{subModule.id}&nbsp;{subModule.version}</span>{' '}
          </span>
        )
        }
      </Popover>
    );
  }

  render() {
    const { onManualUpgrade, hasError } = this.props;
    const nodes = this.props.nodeDescriptions.getActiveNodeDescriptions;

    return (
      <div>
        <h3>Nodes list</h3>
        {hasError &&
          <div className="alert alert-danger" role="alert">
            <span className="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
            <span> Could not connect to the server</span>
          </div>
        }
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Leader</th>
              <th>Address</th>
              <th>Template</th>
              <th>Container</th>
              <th>Modules</th>
              <th>Roles</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
          {nodes && nodes.map((node) =>
            <tr key={`${node.nodeId}`}>
              <td>{node.isClusterLeader ? <i className="fa fa-check-circle" aria-hidden="true"></i> : ''}</td>
              <td>{node.nodeAddress.host}:{node.nodeAddress.port}</td>
              <td>
                {node.nodeTemplate}
              </td>
              <td>
                {node.containerType}
              </td>
              <td>
                {node.isInitialized &&
                  <OverlayTrigger trigger="click" rootClose placement="bottom" overlay={this.nodePopover(node)}>
                    <Button className="btn-info btn-xs">
                      <Icon name="search" />
                    </Button>
                  </OverlayTrigger>
                }
              </td>
              <td>
                {node.roles.map((role) => this.drawRole(node, role))}
              </td>
              {node.isInitialized &&
                <td>
                  <span className="label">{node.isInitialized}</span>
                  {this.props.upgradeNodePrivilege &&
                    <span>
                      {!node.isObsolete &&
                        <button
                        type="button" className="upgrade btn btn-xs btn-success"
                        title="Upgrade Node"
                        onClick={() => onManualUpgrade && onManualUpgrade(node)}>
                          <Icon name="refresh" /> Actual
                        </button>
                      }
                      {node.isObsolete &&
                        <button
                        type="button" className="upgrade btn btn-xs btn-warning"
                        title="Upgrade Node"
                        onClick={() => onManualUpgrade && onManualUpgrade(node)}>
                          <Icon name="refresh" /> Obsolete
                        </button>
                      }
                    </span>
                  }
                  {!this.props.upgradeNodePrivilege &&
                    <span>
                      {!node.isObsolete &&
                        <span className="label label-success">Actual</span>
                      }
                      {node.isObsolete &&
                        <span className="label label-warning">Obsolete</span>
                      }
                    </span>
                  }
                </td>
              }
              {!node.isInitialized &&
                <td>
                  <span className="label label-info">Uncontrolled</span>
                </td>
              }
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
  NodesList,
  {
    fragments: {
      nodeDescriptions: () => Relay.QL`fragment on ClusterKitNodeApi_ClusterKitNodeManagement {
        getActiveNodeDescriptions
        {
          containerType,
          isClusterLeader,
          isObsolete,
          isInitialized,
          leaderInRoles,
          nodeId,
          nodeTemplate,
          nodeTemplateVersion,
          roles,
          startTimeStamp,
          nodeAddress {
            host,
            port,
          },
          modules {
            id,
            version,
          }
        }
      }
      `,
    },
  },
)
