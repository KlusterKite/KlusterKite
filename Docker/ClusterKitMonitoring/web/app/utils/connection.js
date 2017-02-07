import axios from 'axios';
import Cookies from 'js-cookie';
import qs from 'qs';
import moment from 'moment';

/**
 * Get adios connection instance with authorization access token in the header
 * @param {string} accessToken Access token
 */
const getInstance = (accessToken => {
  return axios.create({
    timeout: 5000,
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`,
    },
  });
});

/**
 * Refreshing access token with refresh token, see
 * https://www.oauth.com/oauth2-servers/access-tokens/refreshing-access-tokens/
 * @param {string} refreshToken Refresh token
 */
const requestNewToken = (refreshToken => {
  const instance = axios.create({
    timeout: 5000,
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
  });

  const payload = {
    grant_type: 'refresh_token',
    client_id: 'ClusterKit.NodeManager.WebApplication',
    refresh_token: refreshToken,
  };

  return instance.post('/api/1.x/security/token', qs.stringify(payload));
});

/**
 * Redirects to the authorization page (in case everything else fails)
 * @param reject Callback function
 */
const redirectToAuth = (reject) => {
  localStorage.removeItem('privileges');
  Cookies.remove('accessToken');
  Cookies.remove('refreshToken');
  Cookies.remove('username');
  window.location = '/clusterkit/auth/?from=' + encodeURI(window.location.pathname);
  reject();
};

export const processError = (e) => {
  if (e && e.response && e.response.status) {
    if (e.response.status === 401 || e.response.status === 404) {
      const refreshToken = Cookies.get('refreshToken');
      if (refreshToken) {
        requestNewToken(refreshToken).then(data => {
          processToken(data, resolve);
        }).catch(error => {
          redirectToAuth();
        });
      }
    }
  }

  console.log(e);
};

/**
 * Process new token from the server and resolve new adios connection
 * @param data {Object} Authorization data from the server
 * @param resolve {Function} Resolve function
 */
const processToken = function (data, resolve) {
  const expiresDate = moment().add(data.data.expires_in, 'seconds');
  Cookies.set('accessToken', data.data.access_token, {expires: expiresDate.toDate()});
  Cookies.set('refreshToken', data.data.refresh_token, {expires: 1});

  const instance = getInstance(data.data.access_token);
  resolve(instance);
};

const promise = new Promise((resolve, reject) => {
  const accessToken = Cookies.get('accessToken');
  const refreshToken = Cookies.get('refreshToken');

  if (accessToken && refreshToken){
    const instance = getInstance(accessToken);
    resolve(instance);
  }

  if (!accessToken) {
    if (refreshToken) {
      requestNewToken(refreshToken).then(data => {
        processToken(data, resolve);
      }).catch(error => {
        redirectToAuth();
      });

    } else {
      redirectToAuth(reject);
    }
  }
});

export default promise;
