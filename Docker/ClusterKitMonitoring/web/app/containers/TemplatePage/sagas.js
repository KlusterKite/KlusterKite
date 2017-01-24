import { take, call, put, fork, cancel } from 'redux-saga/effects';
import { takeEvery } from 'redux-saga';

import {
  TEMPLATE_LOAD,
  TEMPLATE_CREATE,
  TEMPLATE_UPDATE,
} from './constants';

import {
  templateReceiveAction,
  templateUpdatedAction,
} from './actions';

import {
  getTemplate,
  createTemplate,
  updateTemplate,
} from './api';

import { LOCATION_CHANGE } from 'react-router-redux';


function* templateLoadSaga(id) {
  const result = yield call(getTemplate, id);
  if (result != null) {
    yield put(templateReceiveAction(result));
  }
}

function* templateCreateSaga(template) {
  try {
    const result = yield call(createTemplate, template);
    yield put(templateUpdatedAction(result, null));
  } catch (error) {
    yield put(templateUpdatedAction(null, error));
  }
}

function* templateUpdateSaga(template) {
  try {
    const result = yield call(updateTemplate, template);
    yield put(templateUpdatedAction(result, null));
  } catch (error) {
    // console.log('update error', `${error}`);
    yield put(templateUpdatedAction(null, `${error}`));
  }
}

function* selectSaga(action) {
  switch (action.type) {
    case TEMPLATE_LOAD:
      yield call(templateLoadSaga, action.id);
      break;
    case TEMPLATE_UPDATE:
      yield call(templateUpdateSaga, action.template, action.onSuccess, action.onError);
      break;
    case TEMPLATE_CREATE:
      yield call(templateCreateSaga, action.template, action.onSuccess, action.onError);
      break;
    default:
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      TEMPLATE_LOAD,
      TEMPLATE_UPDATE,
      TEMPLATE_CREATE,
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

