import { take, call, put, fork, cancel } from 'redux-saga/effects';
import { takeEvery } from 'redux-saga';

import {
  FEED_LOAD,
  FEED_CREATE,
  FEED_UPDATE,
} from './constants';

import {
  feedReceiveAction,
  feedUpdatedAction,
} from './actions';

import {
  getFeed,
  createFeed,
  updateFeed,
} from './api';

import { LOCATION_CHANGE } from 'react-router-redux';


function* feedLoadSaga(id) {
  const result = yield call(getFeed, id);
  if (result != null) {
    yield put(feedReceiveAction(result));
  }
}

function* feedCreateSaga(feed) {
  try {
    const result = yield call(createFeed, feed);
    yield put(feedUpdatedAction(result, null));
  } catch (error) {
    yield put(feedUpdatedAction(null, error));
  }
}

function* feedUpdateSaga(feed) {
  try {
    const result = yield call(updateFeed, feed);
    yield put(feedUpdatedAction(result, null));
  } catch (error) {
    console.log('update error', `${error}`);
    yield put(feedUpdatedAction(null, `${error}`));
  }
}

function* selectSaga(action) {
  switch (action.type) {
    case FEED_LOAD:
      yield call(feedLoadSaga, action.id);
      break;
    case FEED_UPDATE:
      yield call(feedUpdateSaga, action.feed, action.onSuccess, action.onError);
      break;
    case FEED_CREATE:
      yield call(feedCreateSaga, action.feed, action.onSuccess, action.onError);
      break;
    default:
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      FEED_LOAD,
      FEED_UPDATE,
      FEED_CREATE,
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
