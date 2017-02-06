import instance, { processError } from '../../utils/connection';

export function getNodeDescriptions() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/nodemanager/getDescriptions')
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
    console.log('Authorization error', error);
    redirectToAuth();
  });
}

export function upgradeNode(nodeAddress) {
  return instance.then(result => {
    return result.post('/api/1.x/clusterkit/nodemanager/upgradeNode', nodeAddress)
      .then(() => true)
      .catch(error => console.error(error) || false);
  }, error => {
    console.log('Authorization error', error);
    redirectToAuth();
  });
}

export function reloadPackages() {
  return instance.then(result => {
    return result.post('/api/1.x/clusterkit/nodemanager/reloadPackages')
      .then(() => true)
      .catch(error => console.error(error) || false);
  }, error => {
    console.log('Authorization error', error);
    redirectToAuth();
  });
}


export function getSwaggerList() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/swagger/monitor/getList')
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
    console.log('Authorization error', error);
    redirectToAuth();
  });
}
