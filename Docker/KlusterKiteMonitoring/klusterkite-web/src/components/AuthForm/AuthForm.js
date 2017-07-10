import React from 'react';
import { Input } from 'formsy-react-components';

import Form from '../Form/Form';

import './styles.css';

class AuthForm extends React.Component { // eslint-disable-line react/prefer-stateless-function
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    initialValues: React.PropTypes.object,
    authorizing: React.PropTypes.bool,
    authorized: React.PropTypes.bool,
    authorizationError: React.PropTypes.string,
  };

  submit(model) {
    this.props.onSubmit(model);
  }

  render() {
    return (
      <div className="authForm">
        <Form onSubmit={this.submit}
              className="form-horizontal form-margin"
              saving={this.props.authorizing}
              disabled={this.props.authorized}
              saved={this.props.authorized}
              saveError={this.props.authorizationError}
              buttonText="Login"
              savedText="Authorized"
              submitOnEnter={true}
        >
          <fieldset>
            <Input name="Username" label="Username" required elementWrapperClassName="col-sm-2" value="" disabled={this.props.authorized} />
            <Input name="Password" label="Password" required elementWrapperClassName="col-sm-2" value="" disabled={this.props.authorized} type="password" />
          </fieldset>
        </Form>
      </div>
    );
  }
}

export default AuthForm;
