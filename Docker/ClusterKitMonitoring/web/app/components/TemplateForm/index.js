import {autobind} from 'core-decorators'
import React, {Component, PropTypes} from 'react';
import {reduxForm, Field, FieldArray, SubmissionError } from 'redux-form/immutable';
import templateValidation from './TemplateValidation';
import {Hidden, Input, MultipleInput, Submit, TextArea} from '../Forms/index';

import styles from './styles.css';


@reduxForm({
  form: 'template',
  enableReinitialize: true,
  validate: templateValidation,

})
export default class TemplateForm extends Component {

  @autobind
  submit(values) {
    const {onSave} = this.props;
    return new Promise((resolve, reject) => onSave(values.toJS(), resolve, (error) => reject(new SubmissionError({_error: error}))));
  }

  render() {
    const { handleSubmit, pristine, reset, submitting, error, submitSucceeded, valid} = this.props;

    return (
      <div className={styles.templateForm}>
        <form onSubmit={handleSubmit}>
          <Field component={Hidden} name="Id"/>
          <Field component={Hidden} name="Version"/>
          <Field component={Input} name="Code" label="Code" size="medium"/>
          <Field component={Input} name="Name" label="Name"/>
          <Field component={Input} name="MininmumRequiredInstances" label="Minimum Needed Instances" size="small"/>
          <Field component={Input} name="MaximumNeededInstances" label="Maximum Needed Instances" size="small"/>
          <Field component={Input} name="Priority" label="Priority" size="small"/>
          <FieldArray component={MultipleInput} name="Packages" label="Packages" size="medium"/>
          <FieldArray component={MultipleInput} name="ContainerTypes" label="Container Types" size="medium"/>
          <Field component={TextArea} name="Configuration" label="Configuration" rows={10}/>
          <Submit onClick={handleSubmit(this.submit)} saving={submitting} saved={submitSucceeded} saveError={error} valid={valid && !pristine && !submitting} text="Save"/>
        </form>
      </div>
    );
  }
}
