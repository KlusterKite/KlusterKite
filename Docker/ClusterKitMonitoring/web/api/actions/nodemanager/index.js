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