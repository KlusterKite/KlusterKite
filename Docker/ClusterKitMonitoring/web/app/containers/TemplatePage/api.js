import axios from 'axios'

var instance = axios.create({
  timeout: 5000,
  headers: {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  }
});

export function getTemplates() {
  return instance.get('/api/1.x/clusterkit/nodemanager/templates')
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}

export function getTemplate(id) {
  return instance.get('/api/1.x/clusterkit/nodemanager/templates/' + id)
    .then(r => r.data)
    .catch(error => console.error(error) || null);
}

export function updateTemplate(template) {
  return instance.patch('/api/1.x/clusterkit/nodemanager/templates/' + template.Id, template)
    .then(r => r.data);
}

export function createTemplate(template) {
  return instance.put('/api/1.x/clusterkit/nodemanager/templates/', template)
    .then(r => r.data);
}