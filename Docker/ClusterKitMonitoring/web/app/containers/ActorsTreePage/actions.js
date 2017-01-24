/*
 *
 * ActorsTreePage actions
 *
 */

import {
  TREE_LOAD,
  TREE_LOAD_ERROR,
  TREE_LOAD_CLEAN_ERROR,
  TREE_RECEIVE,
  TREE_SCAN,
} from './constants';

export function treeLoadAction() {
  return {
    type: TREE_LOAD,
  };
}

export function treeLoadErrorAction() {
  return {
    type: TREE_LOAD_ERROR,
  };
}

export function treeLoadCleanErrorAction() {
  return {
    type: TREE_LOAD_CLEAN_ERROR,
  };
}

export function treeScanAction() {
  return {
    type: TREE_SCAN,
  };
}

export function treeReceiveAction(tree) {
  return {
    type: TREE_RECEIVE,
    tree,
  };
}
