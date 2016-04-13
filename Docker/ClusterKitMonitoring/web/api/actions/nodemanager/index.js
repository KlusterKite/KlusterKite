import superagent from 'superagent';
import config from '../../../src/config';

function formatUrl(path) {
  const adjustedPath = path[0] !== '/' ? '/' + path : path;
  return 'http://' + config.serverApiHost + ':' + config.serverApiPort + adjustedPath;
}

export function getDescriptions(req) {
  return new Promise((resolve, reject) => {
    const request = superagent['get'](formatUrl('/nodemanager/getDescriptions'));

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}

export function upgradeNode(req) {
  return new Promise((resolve, reject) => {
    const request = superagent['post'](formatUrl('/nodemanager/upgradeNode'));
    request.set('Accept', 'application/json');
    request.send(req.body);

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}

export function getPackages(req) {
  return new Promise((resolve, reject) => {
    const request = superagent['get'](formatUrl('/nodemanager/getPackages'));
    request.set('Accept', 'application/json');

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}

export function reloadPackages(req) {
  return new Promise((resolve, reject) => {
    const request = superagent['post'](formatUrl('/nodemanager/reloadPackages'));
    request.set('Accept', 'application/json');

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}