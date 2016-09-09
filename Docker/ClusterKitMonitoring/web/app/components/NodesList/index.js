/**
*
* NodesList
*
*/

import React, {Component} from 'react';

import styles from './styles.css';


export default class NodesList extends Component {

  drawRole(node, role) {
    const isLeader = node.LeaderInRoles.indexOf(role) >= 0;
    return <span key={node.NodeId + '/' + role}>
                {isLeader && <span className="label label-info" title={role + ' leader'}>{role}</span>}
      {!isLeader && <span className="label label-default">{role}</span>}
      {' '}
           </span>

  }

  render() {
    const {nodes, onManualUpgrade} = this.props;


    return (
      <div className={styles.nodesList}>
        <h2>Nodes list</h2>
        <table className="table table-hover">
          <thead>
          <tr>
            <th>Address</th>
            <th>Leader</th>
            <th>Modules</th>
            <th>Roles</th>
            <th>Status</th>
            <th>Template</th>
            <th>Container</th>
          </tr>
          </thead>
          <tbody>
          {nodes && nodes.map((node) =>
            <tr key={node.NodeId}>
              <td>{node.NodeAddress.Host}:{node.NodeAddress.Port}</td>
              <td>{node.IsClusterLeader ? <i className="fa fa-check-circle" aria-hidden="true"></i> : ''}</td>
              <td>
                {node.Modules.map((subModule) =>
                  <span key={node.NodeId + '/' + subModule.Id}>
                          <span className="label label-default">{subModule.Id}&nbsp;{subModule.Version}</span>{' '}
                        </span>
                )
                }
              </td>
              <td>
                {node.Roles.map((role) => this.drawRole(node, role))}
              </td>
              {node.IsInitialized &&
              <td>
                <span className="label">{node.IsInitialized}</span>
                {!node.IsObsolete &&
                <span className="label label-success">Actual</span>
                }
                {node.IsObsolete &&
                <span className="label label-warning">Obsolete</span>
                }
                <br />
                <button type="button" className={styles.upgrade + ' btn btn-xs'}
                        onClick={() => onManualUpgrade && onManualUpgrade(node)}>
                  <i className="fa fa-refresh"/> {' '}
                  Upgrade Node
                </button>
              </td>
              }
              {!node.IsInitialized &&
              <td>
                <span className="label label-info">Uncontrolled</span>
              </td>
              }
              <td>
                {node.NodeTemplate}
              </td>
              <td>
                {node.ContainerType}
              </td>
            </tr>
          )
          }
          </tbody>
        </table>

      </div>
    );
  }
}

