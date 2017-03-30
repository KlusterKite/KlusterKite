import React from 'react';
import ReactDOM from 'react-dom';
import Relay from 'react-relay'

import Store from './utils/store';

import RoutesList from './routesList';

import './index.css';

const relayInstance = Store.getCurrent();
Relay.injectNetworkLayer(relayInstance);

ReactDOM.render(
  <RoutesList />
  , document.getElementById('root')
);
