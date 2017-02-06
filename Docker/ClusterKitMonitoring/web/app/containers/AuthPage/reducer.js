/*
 *
 * AuthPage reducer
 *
 */

import { fromJS } from 'immutable';
import Cookies from 'js-cookie';
import {
  AUTH_REQUEST_LOGIN,
  AUTH_ON_LOGIN_SUCCESS,
  AUTH_ON_LOGIN_FAILURE,
  AUTH_REQUEST_PRIVELEGES,
  AUTH_ON_PRIVILEGES_SUCCESS,
  AUTH_ON_PRIVILEGES_FAILURE,
} from './constants';
import moment from 'moment';

const initialState = fromJS({
  authorized: false,
  authorizing: false,
  authorizationError: null,
  privilegesReceived: false,
});

function authPageReducer(state = initialState, action) {
  switch (action.type) {
    case AUTH_REQUEST_LOGIN:
      return state
        .set('authorizing', true);
    case AUTH_ON_LOGIN_SUCCESS:
      const expiresDate = moment().add(action.data.expires_in, 'seconds');
      Cookies.set('accessToken', action.data.access_token, { expires: expiresDate.toDate() });
      Cookies.set('refreshToken', action.data.refresh_token, { expires: 1 });
      Cookies.set('username', action.data.username);
      return state
        .set('authorized', true)
        .set('authorizing', false)
        .set('accessToken', action.data.access_token)
        .set('refreshToken', action.data.refresh_token)
        .set('username', action.data.username)
        .set('expires_in', expiresDate);
    case AUTH_ON_LOGIN_FAILURE:
      return state
        .set('authorized', false)
        .set('authorizing', false)
        .set('authorizationError', 'Error');
    case AUTH_REQUEST_PRIVELEGES:
      return state
        .set('requestingPrivileges', true);
    case AUTH_ON_PRIVILEGES_SUCCESS:
      localStorage.setItem('privileges', JSON.stringify(action.data));
      return state
        .set('privilegesReceived', true)
        .set('privileges', action.data);
    case AUTH_ON_PRIVILEGES_FAILURE:
      return state
        .set('privilegesReceived', false)
        .set('privileges', null);
    default:
      return state;
  }
}

export default authPageReducer;
