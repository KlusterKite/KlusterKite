import { take, call, put, select, race, fork, cancel } from 'redux-saga/effects';
import { takeEvery, delay } from 'redux-saga';

import {
  FEEDS_LOAD
} from './constants';

import {
  feedsReceiveAction
} from './actions';

import {
  getFeeds
} from '../FeedPage/api';

import { LOCATION_CHANGE } from 'react-router-redux';


function* feedsLoadSaga() {

  const result = yield call(getFeeds);
  if (result != null) {
    yield put(feedsReceiveAction(result));
  }
}


function* selectSaga(action) {
  switch (action.type) {
    case FEEDS_LOAD:
      yield call(feedsLoadSaga);
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      FEEDS_LOAD,
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

