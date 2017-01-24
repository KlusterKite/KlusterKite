import { take, call, put, cancel, fork } from 'redux-saga/effects';
import { takeEvery } from 'redux-saga';

import {
 TEMPLATES_LOAD,
} from './constants';

import {
  templatesReceiveAction,
} from './actions';

import {
  getTemplates,
} from '../TemplatePage/api';

import { LOCATION_CHANGE } from 'react-router-redux';


function* templatesLoadSaga() {
  const result = yield call(getTemplates);
  if (result != null) {
    yield put(templatesReceiveAction(result));
  }
}


function* selectSaga(action) {
  switch (action.type) {
    case TEMPLATES_LOAD:
      yield call(templatesLoadSaga);
      break;
    default:
      // console.warn(`TemplateListPage sagas - unexpected action ${action.type}`);
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      TEMPLATES_LOAD,
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

