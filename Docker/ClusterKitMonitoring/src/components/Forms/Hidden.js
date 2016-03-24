import React, { Component, PropTypes } from 'react';

export default class Hidden extends Component {
  static propTypes = {
    field: PropTypes.object.isRequired
  }

  render() {
    const {field} = this.props;

    return (
      <input type="hidden" id={field.name} {...field}/>
    );
  }
}
