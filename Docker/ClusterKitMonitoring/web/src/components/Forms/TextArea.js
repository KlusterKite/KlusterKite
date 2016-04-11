import React, { Component, PropTypes } from 'react';

export default class TextArea extends Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    label: PropTypes.string.isRequired,
    rows: PropTypes.number,
    size: PropTypes.string
  }

  /**
   * Get the className for selected size of the form field
   */
  getSizeClassName = () => {
    const {size} = this.props;

    switch (size) {
      case 'small':
        return ' col-xs-2 ';

      case 'medium':
        return ' col-xs-4 ';

      default:
        return ' col-xs-8 ';
    }
  }

  render() {
    const {field, label} = this.props;
    const classSize = this.getSizeClassName();
    let {rows} = this.props;
    if (!rows) {
      rows = 5;
    }

    return (
      <div className={'row' + classSize}>
        <div className={'form-group' + (field.error && field.touched ? ' has-error' : '')}>
          <label htmlFor={field.name}>{label}</label>
          <textarea rows={rows} className="form-control" id={field.name} {...field}></textarea>
          {field.error && field.touched && <div className="text-danger">{field.error}</div>}
        </div>
       </div>
    );
  }
}
