import React from 'react';
import { Input } from 'formsy-react-components';

import Form from '../Form/Form';

import './styles.css';

export default class ResetPassword extends React.Component {
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    initialValues: React.PropTypes.object,
    saving: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveError: React.PropTypes.string,
    saveErrors: React.PropTypes.arrayOf(React.PropTypes.string),
    canEdit: React.PropTypes.bool,
  };

  submit(model) {
    this.props.onSubmit(model, this.props.initialValues.uid);
  }

  render() {
    return (
      <div>
        <h3>Reset Password</h3>
        <Form
          onSubmit={this.submit}
          onDelete={this.props.onDelete ? this.props.onDelete : null}
          className="form-horizontal form-margin"
          saving={this.props.saving}
          deleting={this.props.deleting}
          saved={this.props.saved}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
          savedText="Password reset!"
          buttonText="Reset password"
          forbidEdit={!this.props.canEdit}
        >
          <fieldset>
            <Input
              name="password"
              label="New password"
              type="password"
              elementWrapperClassName="col-sm-3"
              value=""
            />
            <Input
              name="passwordCopy"
              label="New password (repeat)"
              type="password"
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
