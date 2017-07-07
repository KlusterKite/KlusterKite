import React from 'react';
import { Input } from 'formsy-react-components';

import Form from '../Form/Form';

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
          <h2>Edit Feed</h2>
        }
        {!initialValues &&
          <h2>Create a new Feed</h2>
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
            <Input name="nugetFeed" label="NuGet Feed" value={(initialValues && initialValues.nugetFeed) || ""} />
          </fieldset>
        </Form>
      </div>
    );
  }
}
