import React, { Component } from 'react';
import Helmet from 'react-helmet';
import Monitoring from '../Monitoring/Monitoring';

export default class Home extends Component {
  render() {
    const styles = require('./Home.scss');
    return (
      <div className={styles.home}>
        <Helmet title="Home"/>

        <Monitoring />
      </div>
    );
  }
}
