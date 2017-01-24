/*
 *
 * ActorsTreePage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  TREE_LOAD,
  TREE_RECEIVE,
  TREE_LOAD_ERROR,
  TREE_LOAD_CLEAN_ERROR,
  TREE_SCAN,
} from './constants';

const initialState = fromJS({
  tree: {},
  hasError: false,
  isLoading: false,
});

function actorsTreePageReducer(state = initialState, action) {
  switch (action.type) {
    case TREE_LOAD:
    case TREE_SCAN:
      return state.set('isLoading', true);
    case TREE_RECEIVE:
      return state.set('tree', action.tree).set('hasError', false).set('isLoading', false);
    case TREE_LOAD_ERROR:
      return state.set('hasError', true).set('isLoading', false);
    case TREE_LOAD_CLEAN_ERROR:
      return state.set('hasError', false).set('isLoading', false);
    default:
      return state;
  }
}

export default actorsTreePageReducer;
