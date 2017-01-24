/*
 *
 * PackagesListPage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  PACKAGES_RECEIVE,
} from './constants';

const initialState = fromJS({
  packages: [],
});

function packagesListPageReducer(state = initialState, action) {
  switch (action.type) {
    case PACKAGES_RECEIVE:
      return state.set('packages', action.packages);
    default:
      return state;
  }
}

export default packagesListPageReducer;
