import React from 'react';
import { Input, CheckboxGroup } from 'formsy-react-components';

import Form from '../Form/Form';

export default class UserForm extends React.Component {
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    onDelete: React.PropTypes.func,
    initialValues: React.PropTypes.object,
    saving: React.PropTypes.bool,
    deleting: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveError: React.PropTypes.string,
    saveErrors: React.PropTypes.arrayOf(React.PropTypes.string),
    canEdit: React.PropTypes.bool,
    roles: React.PropTypes.arrayOf(React.PropTypes.string)
  };

  submit(model) {
    this.props.onSubmit(model);
  }

  render() {
    const { initialValues } = this.props;
    const canEdit = this.props.canEdit;

    const allRoles = this.props.roles.map(x => { return {value: x, label: x}});
    let selectedRoles = initialValues && initialValues.roles && initialValues.roles.edges.map(x => x.node.role.name);
    if (!selectedRoles){
      selectedRoles = [];
    }

    return (
      <div>
        {initialValues && canEdit &&
          <h2>Edit User</h2>
        }
        {initialValues && !canEdit &&
          <h2>View User</h2>
        }
        {!initialValues &&
          <h2>Create a new User</h2>
        }
        <Form
          onSubmit={this.submit}
          onDelete={this.props.onDelete ? this.props.onDelete : null}
          className="form-horizontal form-margin"
          saving={this.props.saving}
          deleting={this.props.deleting}
          saved={this.props.saved}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
          forbidEdit={!this.props.canEdit}
        >
          <fieldset>
            <Input name="uid" value={(initialValues && initialValues.uid) || ""} type="hidden" />
            <Input name="login" label="Login" value={(initialValues && initialValues.login) || ""} required elementWrapperClassName="col-sm-3" />
            <CheckboxGroup
              name="roles"
              value={selectedRoles}
              label="Roles"
              options={allRoles}
            />
          </fieldset>
          <fieldset>
            <label>Change password</label>
            <Input
              name="password"
              label="New password"
              elementWrapperClassName="col-sm-3"
              value=""
            />
            <Input
              name="passwordCopy"
              label="New password (repeat)"
              elementWrapperClassName="col-sm-3"
              validations="equalsField:password"
              value=""
            />
          </fieldset>
        </Form>
      </div>
    );
  }
}
