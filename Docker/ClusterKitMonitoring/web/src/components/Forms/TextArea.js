import React, { Component, PropTypes } from 'react';

export default class TextArea extends Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    label: PropTypes.string.isRequired,
    rows: PropTypes.number
  }

  render() {
    const {field, label} = this.props;
    let {rows} = this.props;
    if (!rows) {
      rows = 5;
    }

    return (
      <div className={'form-group' + (field.error && field.touched ? ' has-error' : '')}>
        <label htmlFor={field.name}>{label}</label>
        <textarea rows={rows} className="form-control" id={field.name} {...field}></textarea>
        {field.error && field.touched && <div className="text-danger">{field.error}</div>}
      </div>
    );
  }
}
