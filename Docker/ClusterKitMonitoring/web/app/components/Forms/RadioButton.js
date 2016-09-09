import React, { Component, PropTypes } from 'react';
import {Field } from 'redux-form/immutable';

export default class RadioButton extends Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    label: PropTypes.string.isRequired,
    options: PropTypes.arrayOf(PropTypes.string)
  }

  render() {
    const {name, input, meta, label, options} = this.props;

    return (
      <div className="form-group">
        <label>{label}</label>
        <div className={'form-group' + (meta.error && meta.touched ? ' has-error' : '')}>
          {Object.keys(options).map((key) =>
            <label><Field name={name} component="input" type="radio" value={key} checked={input.value == key}/> {options[key]}&nbsp;</label>
          )}
          {meta.error && meta.touched && <div className="text-danger">{meta.error}</div>}
        </div>
      </div>
    );
  }
}
