import React from 'react';
import { Input, RadioGroup } from 'formsy-react-components';

import Form from '../Form/index';

export default class FeedForm extends React.Component {
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    onCancel: React.PropTypes.func,
    onDelete: React.PropTypes.func,
    initialValues: React.PropTypes.object,
    saving: React.PropTypes.bool,
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

    const options = [
      { value: 'Public', label: 'Public' },
      { value: 'Private', label: 'Private' },
    ];

    return (
      <div>
        {initialValues &&
          <h2>Edit Feed</h2>
        }
        {!initialValues &&
          <h2>Create a new Feed</h2>
        }
        <Form
          onSubmit={this.submit}
          onCancel={this.props.onCancel ? this.props.onCancel : null}
          onDelete={this.props.onDelete ? this.props.onDelete : null}
          className="form-horizontal form-margin" saving={this.props.saving}
          saved={this.props.saved}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
        >
          <fieldset>
            <Input name="__id" value={(initialValues && initialValues.__id) || ""} type="hidden" />
            <Input name="id" value={(initialValues && initialValues.id) || ""} type="hidden" />
            <Input name="address" label="Address" value={(initialValues && initialValues.address) || ""} required />
            <Input name="userName" label="User name" value={(initialValues && initialValues.userName) || ""} />
            <Input name="password" label="Password" value={(initialValues && initialValues.password) || ""} />

            <RadioGroup
              name="type"
              label="Type"
              value={initialValues && initialValues.type && initialValues.type.toString()}
              options={options}
              required
            />
          </fieldset>
        </Form>
      </div>
    );
  }
}
