import React, {Component, PropTypes} from 'react';
import {reduxForm} from 'redux-form';
import templateValidation from './TemplateValidation';
import {Hidden, Input, MultipleInput, Submit, TextArea} from '../forms/index';

@reduxForm({
  form: 'template',
  fields: ['Id', 'Code', 'Configuration', 'ContainerTypes', 'MaximumNeededInstances', 'MininmumRequiredInstances', 'Name', 'Packages', 'Priority', 'Version'],
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
    valid: PropTypes.bool.isRequired
  }

  render() {
    const {
      fields: {Id, Code, Configuration, ContainerTypes, MaximumNeededInstances, MininmumRequiredInstances, Name, Packages, Priority, Version},
      handleSubmit
      } = this.props;

    // Packages, ContainerTypes - arrays

    return (
      <div>
        <form onSubmit={handleSubmit}>
          <Hidden field={Id} />
          <Input field={Code} label="Code" />
          <Input field={Name} label="Name" />
          <Input field={MininmumRequiredInstances} label="Minimum Needed Instances" />
          <Input field={MaximumNeededInstances} label="Maximum Needed Instances" />
          <Input field={Priority} label="Priority" />
          <Input field={Version} label="Version" />
          <MultipleInput label="Packages" {...Packages} />
          <TextArea field={Packages} label="Packages" rows={3} />
          <TextArea field={ContainerTypes} label="ContainerTypes" rows={3} />
          <TextArea field={Configuration} label="Configuration" rows={10} />
          <Submit onClick={handleSubmit} />
        </form>
      </div>
    );
  }
}

