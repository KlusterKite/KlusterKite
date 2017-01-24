import { autobind } from 'core-decorators';
import React, { Component, PropTypes } from 'react';
import { reduxForm, Field, SubmissionError } from 'redux-form/immutable';
import feedValidation from './FeedValidation';
import { Hidden, Input, RadioButton, Submit } from '../Forms/index';

@reduxForm({
  form: 'feed',
  enableReinitialize: true,
  validate: feedValidation,

})
export default class FeedForm extends Component {

  static propTypes = {
    onSave: PropTypes.func.isRequired,
    handleSubmit: PropTypes.func.isRequired,
    pristine: PropTypes.bool.isRequired,
    submitting: PropTypes.bool.isRequired,
    error: PropTypes.string.isRequired,
    submitSucceeded: PropTypes.bool.isRequired,
    valid: PropTypes.bool.isRequired,
  }

  @autobind
  submit(values) {
    const { onSave } = this.props;
    return new Promise((resolve, reject) => onSave(values.toJS(), resolve, (error) => reject(new SubmissionError({ _error: error }))));
  }

  render() {
    const { handleSubmit, pristine, submitting, error, submitSucceeded, valid } = this.props;

    const options = {
      0: 'Public',
      1: 'Private',
    };

    return (
      <div>
        <form onSubmit={handleSubmit}>
          <Field component={Hidden} name="Id" />
          <Field component={Input} name="Address" label="Address" />
          <Field component={Input} name="UserName" label="User name" />
          <Field component={Input} name="Password" label="Password" />

          <Field component={RadioButton} name="Type" label="Type" options={options} />


          <Submit onClick={handleSubmit(this.submit)} saving={submitting} saved={submitSucceeded} saveError={error} valid={valid && !pristine && !submitting} text="Save" />
        </form>
      </div>
    );
  }
}
