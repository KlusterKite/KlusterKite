/*
 *
 * FeedPage actions
 *
 */


import {
  FEED_CREATE,
  FEED_LOAD,
  FEED_SET_LOADED,
  FEED_RECEIVE,
  FEED_UPDATE,
  FEED_UPDATED
} from './constants';

export function feedCreateAction(feed) {
  return {
    type: FEED_CREATE,
    feed,
  };
}

export function feedUpdateAction(feed) {
  return {
    type: FEED_UPDATE,
    feed,
  };
}

export function feedLoadAction(id) {
  return {
    type: FEED_LOAD,
    id
  };
}

export function feedSetLoadedAction() {
  return {
    type: FEED_SET_LOADED
  };
}

export function feedReceiveAction(feed) {
  return {
    type: FEED_RECEIVE,
    feed
  };
}

export function feedUpdatedAction(feed, error) {
  return {
    type: FEED_UPDATED,
    feed,
    error
  };
}
