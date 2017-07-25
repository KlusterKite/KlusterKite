import React from 'react';
import isEqual from 'lodash/isEqual';

import Form from '../Form/Form';
import PackagesSelector from '../PackageSelector/PackagesSelector';

export default class SinglePackageForm extends React.Component { // eslint-disable-line react/prefer-stateless-function
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);

    this.state = {
      model: {}
    };
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    onCancel: React.PropTypes.func,
    onDelete: React.PropTypes.func,
    initialValues: React.PropTypes.object,
    saving: React.PropTypes.bool,
    deleting: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveErrors: React.PropTypes.arrayOf(React.PropTypes.string),
    saveError: React.PropTypes.string,
  };

  componentWillMount() {
    this.onReceiveProps(this.props, true);
  }

  componentWillReceiveProps(nextProps) {
    this.onReceiveProps(nextProps, false);
  }

  onReceiveProps(nextProps, skipCheck) {
    if (nextProps.initialValues && (!isEqual(nextProps.initialValues, this.props.initialValues) || skipCheck)) {
      this.setState({
        model: nextProps.initialValues
      });

      console.log('model is' , nextProps.initialValues);
    }
  }

  arrayToString(data) {
    return data && this.replaceAll(data.join(), ',', '\n');
  }

  stringToArray(data) {
    return data && data.length > 0 ? data.split('\n') : [];
  }

  replaceAll(value, search, replacement) {
    return value.replace(new RegExp(search, 'g'), replacement);
  }

  onPackageRequirementsChange(data) {
    this.setState({
      packageRequirements: data
    });
  }

  submit(model) {
    console.log('model', model);
    console.log('model in state', this.state.model);

    // model.packageRequirements = this.state.packageRequirements;
    // model.containerTypes = this.stringToArray(model.containerTypes);
    // model.maximumNeededInstances = Number.parseInt(model.maximumNeededInstances, 10);
    // model.minimumRequiredInstances = model.minimumRequiredInstances ? Number.parseInt(model.minimumRequiredInstances, 10) : 0;
    // model.priority = model.priority ? Number.parseInt(model.priority, 10) : 0;
    // this.props.onSubmit(model);
  }

  cancel() {
    this.props.onCancel();
  }

  onChange(value) {
    console.log('value changed to ', value);
    this.setState({
      model: value
    });
  }

  render() {
    const { initialValues } = this.props;

    const packageInitialValues = {
      id: 'KlusterKite.API.Client',
      specificVersion: null
    };

    return (
      <div>
        {initialValues &&
          <h2>Edit Package</h2>
        }
        {!initialValues &&
          <h2>Add a new Package</h2>
        }
        <Form
          onSubmit={this.submit}
          onCancel={this.props.onCancel}
          onDelete={this.props.onDelete ? this.props.onDelete : null}
          className="form-horizontal form-margin"
          saving={this.props.saving}
          deleting={this.props.deleting}
          saved={this.props.saved}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
        >
          <fieldset>
            {this.props.packagesList &&
              <PackagesSelector
                packages={this.props.packagesList}
                onChange={(value) => this.onChange(value)}
                initialValues={this.props.initialValues.package}
              />
            }
          </fieldset>
        </Form>
      </div>
    );
  }
}
