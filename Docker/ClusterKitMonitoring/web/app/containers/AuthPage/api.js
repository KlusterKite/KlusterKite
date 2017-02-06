import qs from 'qs';
import axios from 'axios';

const instance = axios.create({
  timeout: 5000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

export function login(username, password) {
  const payload = {
    grant_type: 'password',
    client_id: 'ClusterKit.NodeManager.WebApplication',
    username: username,
    password: password,
  };

  return instance.post('/api/1.x/security/token', qs.stringify(payload))
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}

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
      'Authorization': `Bearer ${accessToken}`,
    },
  });
});

export function getPrivileges(accessToken) {
  const instance = getInstance(accessToken);
  return instance.get('/api/1.x/clusterkit/nodemanager/authentication/userScope')
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}
