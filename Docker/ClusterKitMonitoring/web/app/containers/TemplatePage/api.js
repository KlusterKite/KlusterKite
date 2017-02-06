import instance from '../../utils/connection';

export function getTemplates() {
  return instance.then(result => {
    return result.get('/api/1.x/clusterkit/nodemanager/templates')
      .then(r => r.data)
      .catch(error => console.error(error) || null);
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function getTemplate(id) {
  return instance.then(result => {
    return result.get(`/api/1.x/clusterkit/nodemanager/templates/${id}`)
      .then(r => r.data)
      .catch(error => console.error(error) || null);
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function updateTemplate(template) {
  return instance.then(result => {
    return result.patch(`/api/1.x/clusterkit/nodemanager/templates/${template.Id}`, template)
      .then(r => r.data);
  }, error => {
    throw new Error('Authorization error', error);
  });
}

export function createTemplate(template) {
  return instance.then(result => {
    return result.put('/api/1.x/clusterkit/nodemanager/templates/', template)
      .then(r => r.data);
  }, error => {
    throw new Error('Authorization error', error);
  });
}
