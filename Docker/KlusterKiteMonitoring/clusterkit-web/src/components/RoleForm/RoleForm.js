import React from 'react';
import { Input, Textarea } from 'formsy-react-components';

import Form from '../Form/Form';

export default class RoleForm extends React.Component {
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

  arrayToString(data) {
    return data && this.replaceAll(data.join(), ',', '\n');
  }

  stringToArray(data) {
    return data && data.length > 0 ? data.split('\n') : [];
  }

  replaceAll(value, search, replacement) {
    return value.replace(new RegExp(search, 'g'), replacement);
  }

  submit(model) {
    model.allowedScope = this.stringToArray(model.allowedScope);
    model.deniedScope = this.stringToArray(model.deniedScope);
    this.props.onSubmit(model);
  }

  render() {
    const { initialValues } = this.props;
    const canEdit = this.props.canEdit;

    return (
      <div>
        {initialValues && canEdit &&
          <h2>Edit Role</h2>
        }
        {initialValues && !canEdit &&
          <h2>View Role</h2>
        }
        {!initialValues &&
          <h2>Create a new Role</h2>
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
            <Input name="name" label="Name" value={(initialValues && initialValues.name) || ""} required elementWrapperClassName="col-sm-3" />
            <Textarea name="allowedScope" label="Allowed scope" value={(initialValues && this.arrayToString(initialValues.allowedScope)) || ""} rows={12} />
            <Textarea name="deniedScope" label="Denied scope" value={(initialValues && this.arrayToString(initialValues.deniedScope)) || ""} rows={12} />
          </fieldset>
        </Form>
      </div>
    );
  }
}
