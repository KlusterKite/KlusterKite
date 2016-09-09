/*
 *
 * FeedsListPage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  FEEDS_RECEIVE,
} from './constants';

const initialState = fromJS({
  feeds: []
});

function feedsListPageReducer(state = initialState, action) {
  switch (action.type) {
    case FEEDS_RECEIVE:
      return state.set('feeds', action.feeds);
    default:
      return state;
  }
}

export default feedsListPageReducer;
