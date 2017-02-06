import instance from '../../utils/connection';

export function getPackages() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/nodemanager/getPackages')
      .then(r => r.data)
      .catch(error => console.error(error) || null);
  }, error => {
    throw new Error('Authorization error', error);
  });
}

