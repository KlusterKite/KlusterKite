import { take, call, put, fork, cancel } from 'redux-saga/effects';
import { takeEvery } from 'redux-saga';

import {
  AUTH_REQUEST_LOGIN,
  AUTH_REQUEST_PRIVELEGES
} from './constants';

import {
  onLoginSuccessAction,
  onLoginFailureAction,
  requstPrivilegesAction,
  onPrivilegesSuccessAction,
  onPrivilegesFailureAction,
} from './actions';

import {
  login,
  getPrivileges,
} from './api';

import { LOCATION_CHANGE } from 'react-router-redux';

function* loginSaga(username, password) {
  const result = yield call(login, username, password);
  const resultWithUserName = {
    ...result,
    username
  };
  if (result != null) {
    yield put(onLoginSuccessAction(resultWithUserName));
    yield put(requstPrivilegesAction(result.access_token));
  } else {
    yield put(onLoginFailureAction());
  }
}

function* getPrivilegesSaga(accessToken) {
  const result = yield call(getPrivileges, accessToken);

  if (result != null) {
    yield put(onPrivilegesSuccessAction(result));
  } else {
    yield put(onPrivilegesFailureAction());
  }
}

function* selectSaga(action) {
  switch (action.type) {
    case AUTH_REQUEST_LOGIN:
      yield call(loginSaga, action.data.Username, action.data.Password);
      break;
    case AUTH_REQUEST_PRIVELEGES:
      yield call(getPrivilegesSaga, action.data);
      break;
    default:
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      AUTH_REQUEST_LOGIN,
      AUTH_REQUEST_PRIVELEGES,
    ],
    selectSaga);
}

function* rootSaga() {
  const watcher = yield fork(defaultSaga);
  yield take(LOCATION_CHANGE);
  yield cancel(watcher);
}

export default [
  rootSaga,
];
