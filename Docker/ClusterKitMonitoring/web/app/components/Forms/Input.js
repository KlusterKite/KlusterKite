import React, { Component, PropTypes } from 'react';

export default class Input extends Component {
  static propTypes = {
    input: PropTypes.object.isRequired,
    name: PropTypes.string.isRequired,
    label: PropTypes.string.isRequired,
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

    const {name, input, meta, label} = this.props;
    const classSize = this.getSizeClassName();

    return (
      <div className="row">
        <div className={'form-group' + classSize + (meta.error && meta.touched ? ' has-error' : '')}>
          <label htmlFor={name}>{label}</label>
          <input type="text" className="form-control" id={name} {...input}/>
          {meta.error && meta.touched && <div className="text-danger">{meta.error}</div>}
        </div>
      </div>
    );
  }
}
