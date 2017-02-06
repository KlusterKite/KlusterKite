import instance, { processError } from '../../utils/connection';

export function getFeeds() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/nodemanager/nugetFeed')
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
      throw new Error('Authorization error', error);
  });
}

export function getFeed(id) {
  return instance.then(result => {
    return result.get(`/api/1.x/clusterkit/nodemanager/nugetFeed/${id}`)
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function updateFeed(feed) {
  return instance.then(result => {
    return result.patch(`/api/1.x/clusterkit/nodemanager/nugetFeed/${feed.Id}`, feed)
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function createFeed(feed) {
  return instance.then(result => {
    return result.put('/api/1.x/clusterkit/nodemanager/nugetFeed/', feed)
      .then(r => r.data)
      .catch(e => { processError(e) });
  }, error => {
    throw new Error('Authorization error', error);
  });
}
