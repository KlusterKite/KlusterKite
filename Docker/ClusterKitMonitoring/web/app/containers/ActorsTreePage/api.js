/**
 * Created by Kantora on 12.09.2016.
 */
import axios from 'axios';

const instance = axios.create({
  timeout: 5000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

export function getTree() {
  return instance.get('/api/1.x/clusterkit/monitoring/getScanResult')
    .then(r => r.data);
}

export function initScan() {
  return instance.post('/api/1.x/clusterkit/monitoring/initiateScan')
    .then(() => true);
}

