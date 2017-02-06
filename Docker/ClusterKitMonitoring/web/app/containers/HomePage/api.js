import instance from '../../utils/connection';

export function getNodeDescriptions() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/nodemanager/getDescriptions')
      .then(r => r.data);
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function upgradeNode(nodeAddress) {
  return instance.then(result => {
    return result.post('/api/1.x/clusterkit/nodemanager/upgradeNode', nodeAddress)
      .then(() => true)
      .catch(error => console.error(error) || false);
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function reloadPackages() {
  return instance.then(result => {
    return result.post('/api/1.x/clusterkit/nodemanager/reloadPackages')
      .then(() => true)
      .catch(error => console.error(error) || false);
  }, error => {
    throw new Error('Authorization error', error);
  });
}


export function getSwaggerList() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/swagger/monitor/getList')
      .then(r => r.data);
  }, error => {
    throw new Error('Authorization error', error);
  });
}
