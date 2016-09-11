import axios from 'axios';

const instance = axios.create({
  timeout: 5000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

export function getNodeDescriptions() {
  return instance.get('/api/1.x/clusterkit/nodemanager/getDescriptions')
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}

export function upgradeNode(nodeAddress) {
  return instance.post('/api/1.x/clusterkit/nodemanager/upgradeNode', nodeAddress)
    .then(() => true)
    .catch(error => console.error(error) || false);
}

export function reloadPackages() {
  return instance.post('/api/1.x/clusterkit/nodemanager/reloadPackages')
    .then(() => true)
    .catch(error => console.error(error) || false);
}

