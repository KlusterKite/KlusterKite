import axios from 'axios';

const instance = axios.create({
  timeout: 5000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

export function getPackages() {
  return instance.get('/api/1.x/clusterkit/nodemanager/getPackages')
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}

