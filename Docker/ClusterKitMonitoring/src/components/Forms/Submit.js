import React, { Component, PropTypes } from 'react';

export default class Hidden extends Component {
  static propTypes = {
    onClick: PropTypes.func.isRequired
  }

  render() {
    const {onClick} = this.props;

    return (
      <div className="form-group">
        <div className="col-sm-offset-2 col-sm-10">
          <button className="btn btn-success" onClick={onClick}>
            <i className="fa fa-paper-plane"/> Submit
          </button>
        </div>
      </div>
    );
  }
}
