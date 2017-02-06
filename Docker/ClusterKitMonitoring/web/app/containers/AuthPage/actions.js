/*
 *
 * AuthPage actions
 *
 */

import {
  AUTH_REQUEST_LOGIN,
  AUTH_ON_LOGIN_SUCCESS,
  AUTH_ON_LOGIN_FAILURE,
  AUTH_REQUEST_PRIVELEGES,
  AUTH_ON_PRIVILEGES_SUCCESS,
  AUTH_ON_PRIVILEGES_FAILURE,
} from './constants';

export function requestLogin(data) {
  return {
    type: AUTH_REQUEST_LOGIN,
    data,
  };
}

export function onLoginSuccessAction(data) {
  return {
    type: AUTH_ON_LOGIN_SUCCESS,
    data,
  };
}

export function onLoginFailureAction() {
  return {
    type: AUTH_ON_LOGIN_FAILURE,
  };
}

export function requstPrivilegesAction(data) {
  return {
    type: AUTH_REQUEST_PRIVELEGES,
    data,
  };
}

export function onPrivilegesSuccessAction(data) {
  return {
    type: AUTH_ON_PRIVILEGES_SUCCESS,
    data,
  };
}

export function onPrivilegesFailureAction() {
  return {
    type: AUTH_ON_PRIVILEGES_FAILURE,
  };
}
