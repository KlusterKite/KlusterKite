import instance, { processError } from '../../utils/connection';

export function getPackages() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/nodemanager/getPackages')
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
    throw new Error('Authorization error', error);
  });
}

