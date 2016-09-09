import React, { Component, PropTypes } from 'react';

export default class Option extends Component {
  static propTypes = {
    field: PropTypes.object.isRequired,
    item: PropTypes.string.isRequired
  }

  render() {
    const {field, item} = this.props;

    return (
      <label className="radio-inline" key={field.name + '-' + item.toLowerCase()}>
        <input type="radio" {...field} value={item.toLowerCase()} checked={field.value === item.toLowerCase()} /> {item}
      </label>
    );
  }
}
