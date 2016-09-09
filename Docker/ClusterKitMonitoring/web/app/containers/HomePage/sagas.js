import { take, call, put, select, race, fork, cancel } from 'redux-saga/effects';
import { takeEvery, delay } from 'redux-saga';

import {
  NODE_UPGRADE,
  NODE_DESCRIPTIONS_LOAD,
  NODE_RELOAD_PACKAGES
} from './constants';

import {
  nodeDescriptionsReceiveAction
} from './actions';

import {
  getNodeDescriptions,
  upgradeNode,
  reloadPackages
} from './api';

import { LOCATION_CHANGE } from 'react-router-redux';


function* nodeDescriptionsLoadSaga() {
  let cont = true;
  while(cont) {
    const result = yield call(getNodeDescriptions);

    if (result != null) {
      yield put(nodeDescriptionsReceiveAction(result));
    }

    const { cancel: isCancel } = yield race({
      cancel: take(LOCATION_CHANGE),
      timeout: call(delay, 2000)
    });

    if (isCancel) {
      cont = false;
    }
  }
}


function* nodeUpgradeSaga(node) {
  yield call(upgradeNode, node.NodeAddress);
}

function* nodeReloadPackagesSaga(node) {
  yield call(reloadPackages);
}

function* selectSaga(action) {
  switch (action.type) {
    case NODE_DESCRIPTIONS_LOAD:
      yield call(nodeDescriptionsLoadSaga);
      break;
    case NODE_UPGRADE:
      yield call(nodeUpgradeSaga, action.node);
      break;
    case NODE_RELOAD_PACKAGES:
      yield call(nodeReloadPackagesSaga, action.node);
      break;
  }
}

function* defaultSaga() {
  yield* takeEvery(
    [
      NODE_DESCRIPTIONS_LOAD,
      NODE_UPGRADE,
      NODE_RELOAD_PACKAGES
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

