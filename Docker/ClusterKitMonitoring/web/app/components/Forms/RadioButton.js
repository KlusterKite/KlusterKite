import React, { Component, PropTypes } from 'react';
import { Field } from 'redux-form/immutable';

export default class RadioButton extends Component { // eslint-disable-line react/prefer-stateless-function
  static propTypes = {
    name: PropTypes.string,
    input: PropTypes.object.isRequired,
    meta: PropTypes.object.isRequired,
    label: PropTypes.string.isRequired,
    options: PropTypes.object,
  }

  render() {
    const { name, input, meta, label, options } = this.props;

    return (
      <div className="form-group">
        <label>{label}</label>
        <div className={`form-group${(meta.error && meta.touched ? ' has-error' : '')}`}>
          {Object.keys(options).map((key) =>
            <label key={key}><Field name={name} component="input" type="radio" value={key} checked={input.value == key} /> {options[key]}&nbsp;</label> // eslint-disable-line eqeqeq
          )}
          {meta.error && meta.touched && <div className="text-danger">{meta.error}</div>}
        </div>
      </div>
    );
  }
}
