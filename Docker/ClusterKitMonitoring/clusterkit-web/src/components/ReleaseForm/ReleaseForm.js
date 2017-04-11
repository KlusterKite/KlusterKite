import React from 'react';
import { Input, Textarea } from 'formsy-react-components';

import Form from '../Form/Form';

export default class ReleaseForm extends React.Component {
  constructor(props) {
    super(props);
    this.submit = this.submit.bind(this);
  }

  static propTypes = {
    onSubmit: React.PropTypes.func.isRequired,
    onDelete: React.PropTypes.func,
    initialValues: React.PropTypes.object,
    saving: React.PropTypes.bool,
    deleting: React.PropTypes.bool,
    saved: React.PropTypes.bool,
    saveError: React.PropTypes.string,
    saveErrors: React.PropTypes.arrayOf(React.PropTypes.string),
    canEdit: React.PropTypes.bool
  };

  submit(model) {
    model.minorVersion = model.minorVersion ? Number.parseInt(model.minorVersion, 10) : 0;
    model.majorVersion = model.majorVersion ? Number.parseInt(model.majorVersion, 10) : 0;
    this.props.onSubmit(model);
  }

  render() {
    const { initialValues } = this.props;
    const canEdit = !initialValues || initialValues.state === 'Draft';

    return (
      <div>
        {initialValues && canEdit &&
          <h2>Edit Release</h2>
        }
        {initialValues && !canEdit &&
          <h2>View Release</h2>
        }
        {!initialValues &&
          <h2>Create a new Release</h2>
        }
        <Form
          onSubmit={this.submit}
          onDelete={this.props.onDelete ? this.props.onDelete : null}
          className="form-horizontal form-margin"
          saving={this.props.saving}
          deleting={this.props.deleting}
          saved={this.props.saved}
          saveError={this.props.saveError}
          saveErrors={this.props.saveErrors}
          forbidEdit={!this.props.canEdit}
        >
          <fieldset>
            <Input name="__id" value={(initialValues && initialValues.__id) || ""} type="hidden" />
            <Input name="name" label="Name" value={(initialValues && initialValues.name) || ""} required />
            <Textarea name="notes" label="Notes" value={(initialValues && initialValues.notes) || ""} rows={3} />
            <Input name="minorVersion" label="Minor version" value={(initialValues && initialValues.minorVersion.toString()) || ""}
                   required
                   validations={{isNumeric:true}}
                   validationErrors={{isNumeric: 'You have to type a number'}}
                   elementWrapperClassName="col-sm-2" />
            <Input name="majorVersion" label="Major version" value={(initialValues && initialValues.majorVersion.toString()) || ""}
                   required
                   validations={{isNumeric:true}}
                   validationErrors={{isNumeric: 'You have to type a number'}}
                   elementWrapperClassName="col-sm-2" />
          </fieldset>
        </Form>
      </div>
    );
  }
}

