import React, {Component, PropTypes} from 'react';
import {reduxForm} from 'redux-form';
import templateValidation from './TemplateValidation';
import {Hidden, Input, MultipleInput, Submit, TextArea} from '../Forms/index';

@reduxForm({
  form: 'template',
  fields: [
    'Id',
    'Code',
    'Configuration',
    'ContainerTypes[]',
    'MaximumNeededInstances',
    'MininmumRequiredInstances',
    'Name',
    'Packages[]',
    'Priority',
    'Version'
  ],
  validate: templateValidation
})
export default
class TemplateForm extends Component {
  static propTypes = {
    active: PropTypes.string,
    asyncValidating: PropTypes.bool.isRequired,
    fields: PropTypes.object.isRequired,
    dirty: PropTypes.bool.isRequired,
    handleSubmit: PropTypes.func.isRequired,
    invalid: PropTypes.bool.isRequired,
    pristine: PropTypes.bool.isRequired,
    valid: PropTypes.bool.isRequired,
    saving: PropTypes.bool,
    saved: PropTypes.bool,
    saveError: PropTypes.string
  }

  render() {
    const {
      fields: {Id, Code, Configuration, ContainerTypes, MaximumNeededInstances, MininmumRequiredInstances, Name, Packages, Priority, Version},
      handleSubmit,
      saving,
      saved,
      saveError
      } = this.props;

    // Packages, ContainerTypes - arrays
    return (
      <div>
        <form onSubmit={handleSubmit}>
          <Hidden field={Id} />
          <Input field={Code} label="Code" size="medium" />
          <Input field={Name} label="Name" />
          <Input field={MininmumRequiredInstances} label="Minimum Needed Instances" size="small" />
          <Input field={MaximumNeededInstances} label="Maximum Needed Instances" size="small" />
          <Input field={Priority} label="Priority" size="small" />
          <Input field={Version} label="Version" size="small" />
          <MultipleInput field={Packages} label="Packages" size="medium" />
          <MultipleInput field={ContainerTypes} label="Container Types" size="medium" />
          <TextArea field={Configuration} label="Configuration" rows={10} />
          <Submit onClick={handleSubmit} saving={saving} saved={saved} saveError={saveError} text="Save" />
        </form>
      </div>
    );
  }
}

