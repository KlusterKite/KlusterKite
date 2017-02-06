/**
 * Created by Kantora on 12.09.2016.
 */
import instance, { processError } from '../../utils/connection';

export function getTree() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/monitoring/getScanResult')
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function initScan() {
  return instance.then(result => {
    return result.post('/api/1.x/clusterkit/monitoring/initiateScan')
      .then(() => true)
      .catch(e => { processError(e) });
  }, error => {
    throw new Error('Authorization error', error);
  });
}

