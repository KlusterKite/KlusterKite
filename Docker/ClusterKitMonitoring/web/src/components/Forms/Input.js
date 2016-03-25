import React, { Component, PropTypes } from 'react';

export default class Input extends Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    label: PropTypes.string.isRequired
  }

  render() {
    const {field, label} = this.props;

    return (
      <div className={'form-group' + (field.error && field.touched ? ' has-error' : '')}>
        <label htmlFor={field.name}>{label}</label>
        <input type="text" className="form-control" id={field.name} {...field}/>
        {field.error && field.touched && <div className="text-danger">{field.error}</div>}
      </div>
    );
  }
}
