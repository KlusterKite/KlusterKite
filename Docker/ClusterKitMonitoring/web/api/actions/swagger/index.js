import superagent from 'superagent';
import config from '../../../src/config';

function formatUrl(path) {
  const adjustedPath = path[0] !== '/' ? '/' + path : path;
  return 'http://' + config.serverApiHost + ':' + config.serverApiPort + adjustedPath;
}

export function getList(req) {
  return new Promise((resolve, reject) => {
    const request = superagent['get'](formatUrl('/swagger/monitor/getList'));
    request.set('Accept', 'application/json');

    request.end((err, { body } = {}) => err ? reject(body || err) : resolve(body));
  });
}