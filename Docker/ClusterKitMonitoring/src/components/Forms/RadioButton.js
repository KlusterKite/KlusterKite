import React, { Component, PropTypes } from 'react';
import Option from './Option';

export default class RadioButton extends Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    label: PropTypes.string.isRequired,
    options: PropTypes.arrayOf(PropTypes.string)
  }

  render() {
    const {field, label, options} = this.props;

    return (
      <div className="form-group">
        <label>{label}</label>
        <div className={'form-group' + (field.error && field.touched ? ' has-error' : '')}>
          {options.map((item) =>
            <Option key={item} field={field} item={item} />
          )}
          {field.error && field.touched && <div className="text-danger">{field.error}</div>}
        </div>
      </div>
    );
  }
}
