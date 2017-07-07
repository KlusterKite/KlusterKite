import {
  RelayNetworkLayer,
  authMiddleware,
  urlMiddleware,
  retryMiddleware,
} from 'react-relay-network-layer';

import Authorization from './auth-middleware';

let instance = null;

const refresh = () => {
  instance = new RelayNetworkLayer([
    urlMiddleware({
      url: (req) => '/api/1.x/graphQL',
    }),
    authMiddleware({
      allowEmptyToken: true,
      token: Authorization.getToken,
      tokenRefreshPromise: Authorization.tokenRefreshPromise,
    }),
    retryMiddleware({
      fetchTimeout: 15000,
      retryDelays: (attempt) => Math.pow(2, attempt + 4) * 100, // or simple array [3200, 6400, 12800, 25600, 51200, 102400, 204800, 409600],
      forceRetry: (cb, delay) => { window.forceRelayRetry = cb; console.log('call `forceRelayRetry()` for immediately retry! Or wait ' + delay + ' ms.'); },
      statusCodes: [401, 500, 503, 504]
    }),
  ]);

  return instance;
};

const getInstance = () => (instance || refresh());

export default {
  getCurrent: getInstance.bind(this),
  refresh: refresh.bind(this),
};
