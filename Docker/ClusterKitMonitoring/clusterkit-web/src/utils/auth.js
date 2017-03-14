import Storage from './ttl-storage';
import { browserHistory } from 'react-router'

const doRedirect = true;

/**
 * Redirects to the authorization page (in case everything else fails)
 * @param reject Callback function
 */
const redirectToAuth = (reject) => {
  const currentLocation = browserHistory.getCurrentLocation().pathname;
  const currentSearch = browserHistory.getCurrentLocation().search;

  console.log('redirect to auth');
  Storage.remove('privileges');
  Storage.remove('accessToken');
  Storage.remove('refreshToken');
  Storage.remove('username');

  if (currentLocation.indexOf('/Login') === -1 && doRedirect) {
    browserHistory.push(`/Login/?from=${encodeURIComponent(currentLocation + currentSearch)}`);
  }
  reject();
};

/**
 * Refreshing access token with refresh token, see
 * https://www.oauth.com/oauth2-servers/access-tokens/refreshing-access-tokens/
 * @param {string} refreshToken Refresh token
 */
const requestNewToken = (refreshToken => {
  const host = 'http://entry/';
  const url = `${host}api/1.x/security/token`;
  const payload = `grant_type=refresh_token&client_id=ClusterKit.NodeManager.WebApplication&refresh_token=${refreshToken}`;

  return fetch(url, {
    method: 'post',
    headers: { 'Content-Type': 'application/json' },
    body: payload,
    mode: 'cors'
  });
});

/**
 * Save authentication data to the local storage
 * @param data {Object} - Server response
 * @param data.access_token {string} - Access token
 * @param data.refresh_token {string} - Refresh token
 * @param data.expires_in {number} - Access token's expiration time, in seconds
 * @param data.token_type {string} - Represents how an access_token will be generated and presented
 */
const authenticate = (data) => {
  Storage.set('accessToken', data.access_token, data.expires_in * 1000);
  Storage.set('refreshToken', data.refresh_token);
};

/**
 * Process new token from the server and resolve new adios connection
 * @param response {Object} Authorization data from the server
 * @param resolve {Function} Resolve function
 */
const processResponse = (response, resolve) => {
  if (response.status === 200) {
    response.json().then(function(data) {
      authenticate(data);
      resolve(data.access_token);
    });
  } else {
    redirectToAuth();
  }
};

const promise = new Promise((resolve, reject) => {
  const accessToken = Storage.get('accessToken');
  const refreshToken = Storage.get('refreshToken');

  if (accessToken){
    resolve(accessToken);
  }

  if (!accessToken && refreshToken) {
    if (refreshToken) {
      requestNewToken(refreshToken).then(data => {
        processResponse(data, resolve);
      }).catch(error => {
        redirectToAuth();
      });
    } else {
      redirectToAuth(reject);
    }
  }

  if (!accessToken && !refreshToken) {
    redirectToAuth(reject);
  }
});

export default promise;