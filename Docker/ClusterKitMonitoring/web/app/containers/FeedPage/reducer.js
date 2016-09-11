/*
 *
 * FeedPage reducer
 *
 */

import { fromJS } from 'immutable';
import {
  FEED_RECEIVE,
  FEED_UPDATED,
  FEED_NEW,
  FEED_LOAD,
} from './constants';

const initialState = fromJS({
  feed: {
    Id: '0',
    Address: '',
    Password: '',
    UserName: '',
    Type: [],
  },

  updateError: null,
  isLoaded: false,
});

function feedPageReducer(state = initialState, action) {
  switch (action.type) {
    case FEED_LOAD:
      return initialState;
    case FEED_RECEIVE:
      return state.set('feed', action.feed).set('isLoaded', true);
    case FEED_UPDATED:
      return action.feed
        ? state.set('feed', action.feed).set('updateError', null)
        : state.set('updateError', action.error);
    case FEED_NEW:
      return initialState.set('isLoaded', true);
    default:
      return state;
  }
}

export default feedPageReducer;
