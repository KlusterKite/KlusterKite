/*
 *
 * PackagesListPage actions
 *
 */

import {
  PACKAGES_LOAD,
  PACKAGES_RECEIVE,
} from './constants';

export function packagesLoadAction() {
  return {
    type: PACKAGES_LOAD,
  };
}

export function packagesReceiveAction(packages) {
  return {
    type: PACKAGES_RECEIVE,
    packages
  };
}
