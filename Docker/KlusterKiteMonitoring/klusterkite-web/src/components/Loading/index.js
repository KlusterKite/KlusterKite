import React from 'react';

import './styles.css';
import logo from './images/logo.png';

export default class Loading extends React.Component {
  render() {
    return (
      <div>
        <div className="loading">
          <h2>Loading…</h2>
          <img src={logo} alt="Loading" />
          <p>Please wait.</p>
        </div>
      </div>
    );
  }
}
