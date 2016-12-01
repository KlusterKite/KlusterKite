/**
*
* NodesList
*
*/

import React, { Component, PropTypes } from 'react';
import { autobind } from 'core-decorators';
import { Popover, OverlayTrigger, Button } from 'react-bootstrap';

import styles from './styles.css';

export default class NodesList extends Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    nodes: PropTypes.array.isRequired,
    onManualUpgrade: PropTypes.func.isRequired,
    hasError: PropTypes.bool.isRequired,
  }

  drawRole(node, role) {
    const isLeader = node.LeaderInRoles.indexOf(role) >= 0;
    return (<span key={`${node.NodeId}/${role}`}>
                {isLeader && <span className="label label-info" title={`${role} leader`}>{role}</span>}
                {!isLeader && <span className="label label-default">{role}</span>}
                {' '}
    </span>);
  }

  @autobind
  nodePopover(node) {
    return (
      <Popover title={`${node.NodeAddress.Host}:${node.NodeAddress.Port}`} id={`${node.NodeAddress.Host}:${node.NodeAddress.Port}`}>
        {node.Modules.map((subModule) =>
          <span key={`${node.NodeId}/${subModule.Id}`}>
            <span className="label label-default">{subModule.Id}&nbsp;{subModule.Version}</span>{' '}
          </span>
        )
        }
      </Popover>
    );
  }

  render() {
    const { nodes, onManualUpgrade, hasError } = this.props;


    return (
      <div className={styles.nodesList}>
        <h2>Nodes list</h2>
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
            <tr key={`${node.NodeAddress.Host}:${node.NodeAddress.Port}`}>
              <td>{node.IsClusterLeader ? <i className="fa fa-check-circle" aria-hidden="true"></i> : ''}</td>
              <td>{node.NodeAddress.Host}:{node.NodeAddress.Port}</td>
              <td>
                {node.NodeTemplate}
              </td>
              <td>
                {node.ContainerType}
              </td>
              <td>
                {node.IsInitialized &&
                  <OverlayTrigger trigger="click" rootClose placement="bottom" overlay={this.nodePopover(node)}>
                    <Button className="btn-info btn-xs">
                      <span className="fa fa-search"></span>
                    </Button>
                  </OverlayTrigger>
                }
              </td>
              <td>
                {node.Roles.map((role) => this.drawRole(node, role))}
              </td>
              {node.IsInitialized &&
                <td>
                  <span className="label">{node.IsInitialized}</span>
                  {!node.IsObsolete &&
                    <button
                      type="button" className={`${styles.upgrade} btn btn-xs btn-success`}
                      title="Upgrade Node"
                      onClick={() => onManualUpgrade && onManualUpgrade(node)}>
                      <i className="fa fa-refresh" /> Actual
                    </button>
                  }
                  {node.IsObsolete &&
                    <button
                      type="button" className={`${styles.upgrade} btn btn-xs btn-warning`}
                      title="Upgrade Node"
                      onClick={() => onManualUpgrade && onManualUpgrade(node)}>
                      <i className="fa fa-refresh" /> Obsolete
                    </button>
                  }
                </td>
              }
              {!node.IsInitialized &&
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

