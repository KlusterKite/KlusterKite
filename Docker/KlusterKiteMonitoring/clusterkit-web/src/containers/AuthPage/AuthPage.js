import React from 'react';
import { browserHistory } from 'react-router'

import Storage from '../../utils/ttl-storage';

import AuthForm from '../../components/AuthForm/AuthForm';

export default class AuthPage extends React.Component {
  constructor(props) {
    super(props);
    this.login = this.login.bind(this);
    this.authenticate = this.authenticate.bind(this);

    this.state = {
      authorized: false,
      authorizing: false,
      requestingPrivileges: false,
      privilegesReceived: false,
      authorizationError: null,
    };
  }

  /**
   * Try to authenticate user with a username and a password provded
   * @param data {Object} - Authentication data
   * @param data.Username {string} - Username
   * @param data.Password {string} - Password
   */
  login(data) {
    this.setState({
      authorizing: true
    });

    const url = process.env.REACT_APP_AUTH_URL;
    const payload = `grant_type=password&client_id=KlusterKite.NodeManager.WebApplication&username=${data.Username}&password=${data.Password}`;

    fetch(url, {
      method: 'post',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: payload,
      mode: 'cors'
    }).then(response => this.processAuthResponse(response, data.Username));
  }

  /**
   * Process server response for authentication request
   * @param response {Object} - Server HTTP response
   * @param response.json {Function} - Function to get JSON response asynchronously
   * @param username {string} - Username
   */
  processAuthResponse(response, username) {
    const that = this;

    if (response.status === 200) {
      response.json().then(function(data) {
        console.log('got data', data);
        that.authenticate(data, username);
        // that.getPrivileges(data.access_token);
        that.savePrivilegesAndRedirect([]);
      });
    } else {
      this.setState({
        authorizing: false,
        authorized: false,
        authorizationError: 'Login or password incorrect'
      });
    }
  }

  /**
   * Save authentication data to the local storage
   * @param data {Object} - Server response
   * @param data.access_token {string} - Access token
   * @param data.refresh_token {string} - Refresh token
   * @param data.expires_in {number} - Access token's expiration time, in seconds
   * @param data.token_type {string} - Represents how an access_token will be generated and presented
   * @param username {string} - Username
   */
  authenticate(data, username) {
    Storage.set('accessToken', data.access_token, data.expires_in * 1000);
    Storage.set('refreshToken', data.refresh_token);
    Storage.set('username', username);

    this.setState({
      authorizing: false,
      authorized: true
    });
  }

  /**
   * Request privileges list for a given token
   * @param accessToken {string} - Access token
   */
  getPrivileges(accessToken) {
    this.setState({
      requestingPrivileges: true
    });

    // const host = 'http://192.168.99.100/';
    const host = 'http://entry/';
    const url = `${host}api/1.x/klusterkite/nodemanager/authentication/userScope`;

    fetch(url, {
      method: 'get',
      headers: { 'Accept': 'application/json', 'Content-Type': 'application/json', 'Authorization': `Bearer ${accessToken}` },
      mode: 'cors'
    }).then(response => this.processPrivilegesResponse(response));
  }

  /**
   * Process server response for privileges list request
   * @param response {Object} - Server HTTP response
   * @param response.json {Function} - Function to get JSON response asynchronously
   */
  processPrivilegesResponse(response) {
    const that = this;

    console.log('Privileges response', response);
    if (response.status === 200) {
      response.json().then(function(data) {
        that.savePrivilegesAndRedirect(data);
      });
    } else {
      this.setState({
        authorizing: false,
        authorized: false
      });
    }
  }

  /**
   * Saves privileges list to the local storage; redirect user to the page he came from
   * @param data {string[]} - Privileges list
   */
  savePrivilegesAndRedirect(data) {
    Storage.set('privileges', JSON.stringify(data));

    this.setState({
      privilegesReceived: true
    });

    if (this.props.location && this.props.location.query && this.props.location.query.from) {
      browserHistory.push(decodeURIComponent(this.props.location.query.from));
    } else {
      browserHistory.push('/klusterkite/');
    }
  }

  render() {
    return (
      <div className="container">
        <AuthForm
          onSubmit={this.login}
          authorizing={this.state.authorizing}
          authorized={this.state.authorized}
          authorizationError={this.state.authorizationError}
        />
      </div>
    );
  }
}
