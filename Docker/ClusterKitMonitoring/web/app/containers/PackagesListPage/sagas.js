import { take, call, put, select, race, fork, cancel } from 'redux-saga/effects';
import { takeEvery, delay } from 'redux-saga';

import {
  PACKAGES_LOAD
} from './constants';

import {
  packagesReceiveAction
} from './actions';

import {
  getPackages
} from './api';

import { LOCATION_CHANGE } from 'react-router-redux';


function* packagesLoadSaga() {

  const result = yield call(getPackages);
  if (result != null) {
    yield put(packagesReceiveAction(result));
  }
}


function* selectSaga(action) {
  switch (action.type) {
    case PACKAGES_LOAD:
      yield call(packagesLoadSaga);
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      PACKAGES_LOAD,
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

