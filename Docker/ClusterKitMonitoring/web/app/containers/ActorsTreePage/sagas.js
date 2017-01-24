import { take, call, put, fork, cancel } from 'redux-saga/effects';
import { takeEvery, delay } from 'redux-saga';

import {
  TREE_LOAD,
  TREE_SCAN,
} from './constants';

import {
  treeLoadErrorAction,
  treeReceiveAction,
  treeLoadAction,
} from './actions';

import {
  getTree,
  initScan,
} from './api';

import { LOCATION_CHANGE } from 'react-router-redux';


function* treeLoadSaga() {
  try {
    const result = yield call(getTree);
    yield put(treeReceiveAction(result));
  } catch (exception) {
    yield put(treeLoadErrorAction());
  }
}

function* treeScanSaga() {
  try {
    yield call(initScan);
    yield delay(5000);
    yield put(treeLoadAction());
  } catch (exception) {
    yield put(treeLoadErrorAction());
  }
}


function* selectSaga(action) {
  switch (action.type) {
    case TREE_LOAD:
      yield call(treeLoadSaga);
      break;
    case TREE_SCAN:
      yield call(treeScanSaga);
      break;
    default:
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      TREE_LOAD,
      TREE_SCAN,
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

