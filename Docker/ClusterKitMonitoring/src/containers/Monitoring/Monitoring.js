import React, { Component } from 'react';
import MonitoringModules from './MonitoringModules';
import Swagger from './Swagger';

export default class Monitoring extends Component {
  render() {
    return (
        <div className="container">
          <h1>Monitoring</h1>
          <MonitoringModules />
          <Swagger />
        </div>
    );
  }
}
