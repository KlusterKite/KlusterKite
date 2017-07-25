import React from 'react';
import Icon from 'react-fa';
import { Link } from 'react-router';

import './styles.css';

export default class RolesAccess extends React.Component {
  static propTypes = {
    initialValues: React.PropTypes.object,
    roles: React.PropTypes.arrayOf(React.PropTypes.object),
    onGrantOrRevokeRole: React.PropTypes.func,
  };

  /**
   * Grants of revokes role
   * @param roleUid {string} Role uid
   * @param userUid {string} User uid
   * @param hasRole {boolean} Does user have this role at this moment?
   */
  onGrantOrRevokeRole(roleUid, userUid, hasRole) {
    this.props.onGrantOrRevokeRole(roleUid, userUid, hasRole);
  }

  render() {
    const { initialValues } = this.props;

    let selectedRoles = initialValues && initialValues.roles && initialValues.roles.edges.map(x => x.node.role.name);
    if (!selectedRoles){
      selectedRoles = [];
    }

    return (
      <div>
        <h3>Roles access</h3>

        <table className="table table-hover">
          <thead>
          <tr>
            <th className="roles-access"></th>
            <th>Role</th>
            <th></th>
          </tr>
          </thead>
          <tbody>
          {this.props.roles.map((item) => {
            const hasRole = selectedRoles.includes(item.name);

            return (
              <tr key={item.uid}>
                <td className="roles-access">
                  {hasRole &&
                  <Icon name='check-circle' />
                  }
                </td>
                <td>
                  <Link to={`/klusterkite/Roles/${item.id}`}>
                    {item.name}
                  </Link>
                </td>
                <td>
                  {hasRole &&
                  <button className="btn btn-danger" type="button" onClick={() => {this.onGrantOrRevokeRole(item.uid, initialValues.uid, hasRole)}}>
                    <Icon name='minus' />{' '}
                    Revoke
                  </button>
                  }
                  {!hasRole &&
                  <button className="btn btn-success" type="button" onClick={() => {this.onGrantOrRevokeRole(item.uid, initialValues.uid, hasRole)}}>
                    <Icon name='plus' />{' '}
                    Grant
                  </button>
                  }
                </td>
              </tr>
            )
          })}
          </tbody>
        </table>
      </div>
    );
  }
}
