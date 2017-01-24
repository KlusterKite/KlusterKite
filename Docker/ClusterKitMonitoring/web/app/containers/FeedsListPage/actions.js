/*
 *
 * FeedsListPage actions
 *
 */

import {
  FEEDS_LOAD,
  FEEDS_RECEIVE,
} from './constants';

export function feedsLoadAction() {
  return {
    type: FEEDS_LOAD,
  };
}

export function feedsReceiveAction(feeds) {
  return {
    type: FEEDS_RECEIVE,
    feeds,
  };
}
