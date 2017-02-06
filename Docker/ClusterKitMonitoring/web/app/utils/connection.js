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
      'bearer': accessToken,
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
let redirectToAuth = (reject) => {
  window.location = '/clusterkit/auth/?from=' + encodeURI(window.location.pathname);
  reject();
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
        const expiresDate = moment().add(data.data.expires_in, 'seconds');
        Cookies.set('accessToken', data.data.access_token, { expires: expiresDate.toDate() });
        Cookies.set('refreshToken', data.data.refresh_token, { expires: 1 });

        const instance = getInstance(accessToken);
        resolve(instance);
      }).catch(error => {
        redirectToAuth();
      });

    } else {
      redirectToAuth(reject);
    }
  }
});

export default promise;

