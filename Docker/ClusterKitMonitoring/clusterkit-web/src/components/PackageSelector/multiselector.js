import React from 'react';
import isEqual from 'lodash/isEqual';

import PackagesSelector from './selector';

export default class PackagesMultiSelector extends React.Component {

  static propTypes = {
    packages: React.PropTypes.object.isRequired,
    onChange: React.PropTypes.func,
  };

  constructor() {
    super();

    this.state = {
      values: [
        {
          package: '',
          version: ''
        }
      ]
    };
  }

  componentWillMount() {
    this.onReceiveProps(this.props, true);
  }

  componentWillReceiveProps(nextProps) {
    this.onReceiveProps(nextProps, false);
  }

  onReceiveProps(nextProps, skipCheck) {
    if (nextProps.values && (!isEqual(nextProps.values, this.props.values) || skipCheck)) {
      this.setState({
        values: nextProps.values
      })
    }
  }

  onChange(index, value) {
    const newValues = [
      ...this.state.values.slice(0, index),
      value,
      ...this.state.values.slice(index + 1)
    ];

    this.setState({
      values: newValues
    });
    
    if (this.props.onChange) {
      this.props.onChange(newValues);
    }
  }

  onAdd() {
    const newItem = {
      package: '',
      version: ''
    };

    this.setState((prevState, props) => ({
      values: [...prevState.values, newItem]
    }));
  }

  onDelete(index) {
    this.setState((prevState, props) => ({
      values: [
        ...prevState.values.slice(0, index),
        ...prevState.values.slice(index + 1)
      ]
    }));
  }

  render() {
    const recordsCount = this.state.values && this.state.values.length;
    return (
      <div className="form-group row package-selector-outer">
        <label className="control-label col-sm-3" data-required="false">Maximum Needed Instances</label>
        <div className="col-sm-9">
        {this.state.values && this.state.values.length > 0 && this.state.values.map((item, index) => {
            let onDelete = () => this.onDelete(index);
            if (recordsCount === index + 1) {
              onDelete = null;
            }

            let onAdd = () => this.onAdd();
            if (recordsCount !== index + 1) {
              onAdd = null;
            }

            return (<PackagesSelector
              packages={this.props.packages}
              key={index}
              onChange={(value) => this.onChange(index, value)}
              onDelete={onDelete}
              onAdd={onAdd}
              initialValues={item}
            />)
          }
        )}
        </div>
      </div>
    );
  }
}

