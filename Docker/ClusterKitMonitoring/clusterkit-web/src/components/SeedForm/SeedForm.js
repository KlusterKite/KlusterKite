import React from 'react';
import { Input, Textarea } from 'formsy-react-components';

import Form from '../Form/Form';

export default class SeedForm extends React.Component {
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    onCancel: React.PropTypes.func,
    onDelete: React.PropTypes.func,
    initialValues: React.PropTypes.arrayOf(React.PropTypes.string),
    saving: React.PropTypes.bool,
    deleting: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveErrors: React.PropTypes.arrayOf(React.PropTypes.string),
    saveError: React.PropTypes.string,
  };

  submit(model) {
    // model.Type = Number.parseInt(model.Type);
    this.props.onSubmit(model);
  }

  render() {
    const { initialValues } = this.props;

    return (
      <div>
        {initialValues &&
          <h2>Edit Seeds</h2>
        }
        <Form
          onSubmit={this.submit}
          onCancel={this.props.onCancel ? this.props.onCancel : null}
          onDelete={this.props.onDelete ? this.props.onDelete : null}
          className="form-horizontal form-margin"
          saving={this.props.saving}
          deleting={this.props.deleting}
          saved={this.props.saved}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
        >
          <fieldset>
            <Textarea name="seedAddresses" label="Seed Addresses" value={initialValues || ""} rows={6} />
          </fieldset>
        </Form>
      </div>
    );
  }
}
