import React from 'react';
import { browserHistory } from 'react-router'

import Storage from '../../utils/ttl-storage';

export default class LogoutPage extends React.Component {
  componentWillMount() {
    Storage.remove('accessToken');
    Storage.remove('refreshToken');
    Storage.remove('username');
    Storage.remove('privileges');

    browserHistory.push('/klusterkite/Login/');
  }

  render() {
    return (
      <div className="container">
        <h1>Logging out…</h1>
      </div>
    );
  }
}
