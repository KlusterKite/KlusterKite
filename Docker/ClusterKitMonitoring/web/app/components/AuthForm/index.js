/**
*
* AuthForm
*
*/

import { autobind } from 'core-decorators';
import React, { Component, PropTypes } from 'react';

import { reduxForm, Field, SubmissionError } from 'redux-form/immutable';
import authValidation from './AuthValidation';
import { Input, Submit } from '../Forms/index';

import styles from './styles.css';

@reduxForm({
  form: 'auth',
  enableReinitialize: true,
  validate: authValidation,
})
class AuthForm extends React.Component { // eslint-disable-line react/prefer-stateless-function

  static propTypes = {
    onSubmit: PropTypes.func.isRequired,
    handleSubmit: PropTypes.func.isRequired,
    pristine: PropTypes.bool.isRequired,
    submitting: PropTypes.bool,
    error: PropTypes.string,
    submitSucceeded: PropTypes.bool,
    valid: PropTypes.bool.isRequired,
    authorizing: PropTypes.bool.isRequired,
    authorized: PropTypes.bool,
    authorizationError: PropTypes.string,
  }

  @autobind
  submit(values) {
    const { onSubmit } = this.props;
    onSubmit(values.toJS());
    // return new Promise((resolve, reject) => onSubmit(values.toJS(), resolve, (error) => reject(new SubmissionError({ _error: error }))));
  }

  render() {
    const { handleSubmit, pristine, valid, authorizing, authorized, authorizationError } = this.props;

    return (
      <div className={styles.authForm}>
        <form onSubmit={handleSubmit}>
          <Field component={Input} name="Username" label="Username" size="small" />
          <Field component={Input} name="Password" label="Password" type="password" size="small" />

          <Submit onClick={handleSubmit(this.submit)} saving={authorizing} saved={authorized} saveError={authorizationError} valid={valid && !pristine && !authorizing} text="Login" savedText="Authorized" />
        </form>
      </div>
    );
  }
}

export default AuthForm;
