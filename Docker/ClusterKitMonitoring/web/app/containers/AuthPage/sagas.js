import { take, call, put, fork, cancel } from 'redux-saga/effects';
import { takeEvery } from 'redux-saga';

import {
  AUTH_REQUEST_LOGIN
} from './constants';

import {
  onLoginSuccessAction,
  onLoginFailureAction,
} from './actions';

import {
  login,
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
  } else {
    yield put(onLoginFailureAction());
  }
}

function* selectSaga(action) {
  switch (action.type) {
    case AUTH_REQUEST_LOGIN:
      yield call(loginSaga, action.data.Username, action.data.Password);
      break;
    default:
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      AUTH_REQUEST_LOGIN,
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
