import axios from 'axios';

const instance = axios.create({
  timeout: 5000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

export function getFeeds() {
  return instance.get('/api/1.x/clusterkit/nodemanager/nugetFeed')
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}

export function getFeed(id) {
  return instance.get(`/api/1.x/clusterkit/nodemanager/nugetFeed/${id}`)
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}

export function updateFeed(feed) {
  return instance.patch(`/api/1.x/clusterkit/nodemanager/nugetFeed/${feed.Id}`, feed)
    .then(r => r.data);
}

export function createFeed(feed) {
  return instance.put('/api/1.x/clusterkit/nodemanager/nugetFeed/', feed)
    .then(r => r.data);
}
