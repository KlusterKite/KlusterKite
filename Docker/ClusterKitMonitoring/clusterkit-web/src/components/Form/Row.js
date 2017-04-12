import React from 'react';
import { Row } from 'formsy-react-components';

import './styles.css';

export default class Form extends React.Component { // eslint-disable-line react/prefer-stateless-function
  static propTypes = {
    label: React.PropTypes.string.isRequired,
    text: React.PropTypes.string.isRequired,
  };

  render() {
    return (
      <Row label={this.props.label} rowClassName="row" labelClassName="col-sm-3">
        <div className="col-sm-9">
          <p className="row-text">{this.props.text}</p>
        </div>
      </Row>
    );
  }
}
