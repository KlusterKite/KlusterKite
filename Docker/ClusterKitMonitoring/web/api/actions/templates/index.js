import superagent from 'superagent';
import config from '../../../src/config';

function formatUrl(path) {
  const adjustedPath = path[0] !== '/' ? '/' + path : path;
  return 'http://' + config.serverApiHost + ':' + config.serverApiPort + adjustedPath;
}

export function get(req) {
  return new Promise((resolve, reject) => {
    const request = superagent['get'](formatUrl('/nodemanager/templates'));
    request.set('Accept', 'application/json');

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}

export function getById(req) {
  const id = req.url.split('/')[3];
  return new Promise((resolve, reject) => {
    const request = superagent['get'](formatUrl('/nodemanager/templates/' + id));
    request.set('Accept', 'application/json');

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}

export function update(req) {
  const id = req.url.split('/')[3];
  return new Promise((resolve, reject) => {
    const request = superagent['patch'](formatUrl('/nodemanager/templates/' + id));
    request.set('Accept', 'application/json');
    request.send(req.body);

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}

export function create(req) {
  return new Promise((resolve, reject) => {
    const request = superagent['put'](formatUrl('/nodemanager/templates'));
    request.set('Accept', 'application/json');
    request.send(req.body);

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}
