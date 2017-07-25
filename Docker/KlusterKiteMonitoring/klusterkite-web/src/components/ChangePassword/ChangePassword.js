import React from 'react';
import { Input } from 'formsy-react-components';

import Form from '../Form/Form';

export default class ChangePassword extends React.Component {
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
    this.props.onSubmit(model);
  }

  render() {
    return (
      <div>
        <h3>Reset Password</h3>
        <Form
          onSubmit={this.submit}
          className="form-horizontal form-margin"
          saving={this.props.saving}
          saved={this.props.saved}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
          savedText="Password changes!"
          buttonText="Change password"
        >
          <fieldset>
            <Input
              name="passwordOld"
              label="Old password"
              type="password"
              elementWrapperClassName="col-sm-3"
              value=""
            />
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
