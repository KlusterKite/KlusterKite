import React from 'react';
import Formsy from 'formsy-react';
import { Input, Textarea } from 'formsy-react-components';
import Form from '../Form/index';

export default class TemplateForm extends React.Component { // eslint-disable-line react/prefer-stateless-function
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);

    Formsy.addValidationRule('isLessOrEqualThan', function (values, value, otherField) {
      if (isNaN(Number(value))) return true;
      if (isNaN(Number(otherField))) return true;
      return Number(value) <= Number(values[otherField]);
    });

    Formsy.addValidationRule('isMoreOrEqualThan', function (values, value, otherField) {
      if (isNaN(Number(value))) return true;
      if (isNaN(Number(otherField))) return true;
      return Number(value) >= Number(values[otherField]);
    });
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    onDelete: React.PropTypes.func,
    initialValues: React.PropTypes.object,
    saving: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveError: React.PropTypes.string,
  };

  arrayToString(data) {
    return this.replaceAll(data.join(), ',', '\n');
  }

  stringToArray(data) {
    return data && data.length > 0 ? data.split('\n') : [];
  }

  replaceAll(value, search, replacement) {
    return value.replace(new RegExp(search, 'g'), replacement);
  }

  submit(model) {
    model.packages = this.stringToArray(model.packages);
    model.containerTypes = this.stringToArray(model.containerTypes);
    model.maximumNeededInstances = Number.parseInt(model.maximumNeededInstances, 10);
    model.minimumRequiredInstances = model.minimumRequiredInstances ? Number.parseInt(model.minimumRequiredInstances, 10) : 0;
    model.version = model.version ? Number.parseInt(model.version, 10) : 0;
    model.priority = model.priority ? Number.parseInt(model.priority, 10) : 0;
    this.props.onSubmit(model);
  }

  render() {
    const { initialValues } = this.props;
    return (
      <div>
        {initialValues &&
          <h2>Edit Template</h2>
        }
        {!initialValues &&
          <h2>Create a new Template</h2>
        }
        <Form onSubmit={this.submit} onDelete={this.props.onDelete ? this.props.onDelete : null} className="form-horizontal form-margin" saving={this.props.saving} saved={this.props.saved} saveError={this.props.saveError}>
          <fieldset>
            <Input name="__id" value={initialValues && initialValues.__id} type="hidden" />
            <Input name="version" value={(initialValues && initialValues.version) || ""} type="hidden" />
            <Input name="code" label="Code" value={(initialValues && initialValues.code) || ""} required />
            <Input name="name" label="Name" value={(initialValues && initialValues.name) || ""} required />
            <Input name="version" label="Version" value={(initialValues && initialValues.version) || ""} validations="isNumeric" validationError="Must be numeric" elementWrapperClassName="col-sm-2" />
            <Input
              name="minimumRequiredInstances"
              label="Minimum Required Instances"
              value={(initialValues && initialValues.minimumRequiredInstances) || ""}
              validations={{isNumeric:true,isLessOrEqualThan:'maximumNeededInstances'}}
              validationErrors={{isNumeric: 'You have to type a number', isLessOrEqualThan: 'Cannot exceed Maximum Needed Instances'}}
              elementWrapperClassName="col-sm-2"
            />
            <Input
              name="maximumNeededInstances"
              label="Maximum Needed Instances"
              value={(initialValues && initialValues.maximumNeededInstances) || ""}
              validations={{isNumeric:true,isMoreOrEqualThan:'minimumRequiredInstances'}}
              validationErrors={{isNumeric: 'You have to type a number', isMoreOrEqualThan: 'Cannot be less than Minimum Required Instances'}}
              elementWrapperClassName="col-sm-2"
            />
            <Input name="priority" label="Priority" value={(initialValues && initialValues.priority) || ""} validations="isNumeric" validationError="Must be numeric" elementWrapperClassName="col-sm-2" />
            <Textarea name="packages" label="Packages" value={(initialValues && this.arrayToString(initialValues.packages)) || ""} rows={6} />
            <Textarea name="containerTypes" label="Container Types" value={(initialValues && this.arrayToString(initialValues.containerTypes)) || ""} rows={3} />
            <Textarea name="configuration" label="Configuration" value={(initialValues && initialValues.configuration) || ""} rows={10} />
          </fieldset>
        </Form>
      </div>
    );
  }
}
