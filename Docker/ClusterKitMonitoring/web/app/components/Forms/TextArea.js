import React, { Component, PropTypes } from 'react';

export default class TextArea extends Component {
  static propTypes = {
    input: PropTypes.object.isRequired,
    meta: PropTypes.object.isRequired,
    name: PropTypes.string.isRequired,
    label: PropTypes.string.isRequired,
    rows: PropTypes.number,
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

  render() {
    const { name, input, meta, label } = this.props;
    const classSize = this.getSizeClassName();
    let { rows } = this.props;
    if (!rows) {
      rows = 5;
    }

    return (
      <div className={`row${classSize}`}>
        <div className={`form-group${(meta.error && meta.touched ? ' has-error' : '')}`}>
          <label htmlFor={name}>{label}</label>
          <textarea rows={rows} className="form-control" id={name} {...input}></textarea>
          {meta.error && meta.touched && <div className="text-danger">{meta.error}</div>}
        </div>
      </div>
    );
  }
}
