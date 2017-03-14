import React from 'react';
import ReactDOM from 'react-dom';
import Relay from 'react-relay'

import instance from './utils/auth';
import Storage from './utils/ttl-storage';

import RoutesList from './routesList';

import './index.css';

console.log('accessToken', Storage.get('accessToken'));
console.log('refreshToken', Storage.get('refreshToken'));

instance.then(token => {
  console.log('got auth', token);

  Relay.injectNetworkLayer(
    new Relay.DefaultNetworkLayer('http://entry/api/1.x/graphQL', {
      get headers() {
        return {
          Authorization: 'Bearer ' + Storage.get('accessToken')
        }
      }
    })
  );

  ReactDOM.render(
    <RoutesList />
    , document.getElementById('root')
  )
}, error => {
  console.log(error);

  Relay.injectNetworkLayer(
    new Relay.DefaultNetworkLayer('http://entry/api/1.x/graphQL')
  )

  ReactDOM.render(
    <RoutesList />
    , document.getElementById('root')
  )
});
