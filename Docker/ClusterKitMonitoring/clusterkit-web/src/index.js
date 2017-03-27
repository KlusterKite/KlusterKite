import React from 'react';
import ReactDOM from 'react-dom';
import Relay from 'react-relay'

import Storage from './utils/ttl-storage';
import Store from './utils/store';

import RoutesList from './routesList';

import './index.css';

console.log('accessToken', Storage.get('accessToken'));
console.log('refreshToken', Storage.get('refreshToken'));

const relayInstance = Store.getCurrent();
Relay.injectNetworkLayer(relayInstance);

ReactDOM.render(
  <RoutesList />
  , document.getElementById('root')
);
