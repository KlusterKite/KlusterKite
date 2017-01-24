import React, { Component, PropTypes } from 'react';
import { Field } from 'redux-form/immutable';

export default class MultipleInput extends Component {
  static propTypes = {
    fields: PropTypes.object.isRequired,
    meta: PropTypes.object.isRequired,
    label: PropTypes.string.isRequired,
    size: PropTypes.string,
  }

  /**
   * Get the className for selected size of the form field
   */
  getSizeClassName = () => {
    const { size } = this.props;

    switch (size) {
      case 'small':
        return ' col-xs-2 ';

      case 'medium':
        return ' col-xs-4 ';

      default:
        return ' col-xs-8 ';
    }
  }

  /**
   * Process adding new input field to the form
   */
  handleAdd = () => {
    const { fields } = this.props;
    event.preventDefault();    // prevent form submission
    fields.push();
  }

  render() {
    const { fields, label, meta } = this.props;
    const classSize = this.getSizeClassName();

    return (
      <div className="row">
        <div className={classSize}>
          <label>{label} <i className="fa fa-plus-circle" role="button" onClick={this.handleAdd}></i></label>
          {!fields.length && <div>No Items</div>}
          {fields && fields.map((elementName, index) =>
            <div key={index} className={'form-group'}>
              <Field component="input" type="text" className="form-control" id={elementName} name={elementName} />
            </div>
            )}
          {meta.error && meta.touched && <div className="text-danger">{meta.error}</div>}
        </div>
      </div>
    );
  }
}
