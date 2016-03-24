import React, { Component, PropTypes } from 'react';

export default class MultipleInput extends Component {
  static propTypes = {
    label: PropTypes.string.isRequired,
    value: PropTypes.any
  }

  render() {
    const {label, value} = this.props;
    console.log('multiple input');
    console.log(value);
    // const items = field.split(',');
    // console.log(items);

    return (
      <div>
        {label}
      </div>
    );
  }
}
