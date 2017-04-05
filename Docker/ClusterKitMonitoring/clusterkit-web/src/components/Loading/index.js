import React from 'react';

import './styles.css';
import logo from './images/logo.png';

export default class Loading extends React.Component {
  render() {
    return (
      <div>
        <h2>Loading…</h2>
        <p>Please wait.</p>
        <img src={logo} className="loading" />
      </div>
    );
  }
}
