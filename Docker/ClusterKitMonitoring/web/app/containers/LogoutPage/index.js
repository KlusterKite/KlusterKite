/*
 *
 * LogoutPage
 *
 */

import React from 'react';
import { connect } from 'react-redux';

import Cookies from 'js-cookie';

export class LogoutPage extends React.Component { // eslint-disable-line react/prefer-stateless-function
  componentWillMount() {
    Cookies.remove('accessToken');
    Cookies.remove('refreshToken');
    Cookies.remove('username');
    window.location = '/clusterkit/auth/';
  }

  render() {
    return (
      <div>
        Logging out…
      </div>
    );
  }
}


function mapDispatchToProps(dispatch) {
  return {
    dispatch,
  };
}

export default connect(mapDispatchToProps)(LogoutPage);
