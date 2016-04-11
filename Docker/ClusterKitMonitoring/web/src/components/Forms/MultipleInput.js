import React, { Component, PropTypes } from 'react';

export default class MultipleInput extends Component {
  static propTypes = {
    field: PropTypes.arrayOf(PropTypes.object),
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

  /**
   * Process adding new input field to the form
   */
  handleAdd = () => {
    const {field} = this.props;

    event.preventDefault();    // prevent form submission
    field.addField();
  }

  render() {
    const {field, label} = this.props;
    const classSize = this.getSizeClassName();

    return (
      <div className="row">
        <div className={classSize}>
          <label>{label} <i className="fa fa-plus-circle" role="button" onClick={this.handleAdd}></i></label>
          {!field.length && <div>No Items</div>}
          {field && field.map((item, index) =>
            <div key={index} className={'form-group'}>
              <input type="text" className="form-control" id={item.name} {...item}/>
            </div>
          )}
        </div>
      </div>
    );
  }
}
