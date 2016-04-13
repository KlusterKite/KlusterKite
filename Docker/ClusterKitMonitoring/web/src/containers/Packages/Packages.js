import React, { Component } from 'react';
import PackagesList from './PackagesList';

export default class Packages extends Component {
  render() {
    return (
        <div className="container">
          <h1>Packages</h1>
          <PackagesList />
        </div>
    );
  }
}
