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

    String.prototype.replaceAll = function(search, replacement) {
      const target = this;
      return target.replace(new RegExp(search, 'g'), replacement);
    };
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    initialValues: React.PropTypes.object,
    saving: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveError: React.PropTypes.string,
  };

  arrayToString(data) {
    return data.join().replaceAll(',', '\n');
  }

  stringToArray(data) {
    return data && data.length > 0 ? data.split('\n') : [];
  }

  submit(model) {
    model.packages = this.stringToArray(model.packages);
    model.containerTypes = this.stringToArray(model.containerTypes);
    model.maximumNeededInstances = Number.parseInt(model.maximumNeededInstances);
    model.minimumRequiredInstances = Number.parseInt(model.minimumRequiredInstances);
    model.version = Number.parseInt(model.version);
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
        <Form onSubmit={this.submit} className="form-horizontal form-margin" saving={this.props.saving} saved={this.props.saved} saveError={this.props.saveError}>
          <p>Version: {initialValues.version}</p>
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
              required
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
            <Textarea name="bonfiguration" label="Configuration" value={(initialValues && initialValues.configuration) || ""} rows={10} />
          </fieldset>
        </Form>
      </div>
    );
  }
}
