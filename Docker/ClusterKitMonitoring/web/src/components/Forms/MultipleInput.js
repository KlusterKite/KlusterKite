import React, { Component, PropTypes } from 'react';

export default class MultipleInput extends Component {
  static propTypes = {
    field: PropTypes.arrayOf(PropTypes.object),
    label: PropTypes.string.isRequired
  }

  handleAdd = () => {
    const {field} = this.props;

    event.preventDefault();    // prevent form submission
    field.addField();
  }

  render() {
    const {field, label} = this.props;

    return (
      <div className="row">
        <label>{label} <i className="fa fa-plus-circle" role="button" onClick={this.handleAdd}></i></label>
        {!field.length && <div>No Items</div>}
        {field && field.map((item, index) =>
            <div key={index} className="form-group">
              <input type="text" className="form-control" id={item.name} {...item}/>
            </div>
          )}
      </div>
    );
  }
}
