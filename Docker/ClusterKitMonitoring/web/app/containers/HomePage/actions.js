/*
 *
 * HomePage actions
 *
 */

import {
  NODE_DESCRIPTIONS_LOAD,
  NODE_DESCRIPTIONS_RECEIVE,
  NODE_UPGRADE,
  NODE_RELOAD_PACKAGES
} from './constants';

export function nodeDescriptionsLoadAction() {
  return {
    type: NODE_DESCRIPTIONS_LOAD,
  };
}

export function nodeDescriptionsReceiveAction(nodeDescriptions) {
  return {
    type: NODE_DESCRIPTIONS_RECEIVE,
    nodeDescriptions
  };
}

export function nodeUpgradeAction(node) {
  return {
    type: NODE_UPGRADE,
    node
  };
}

export function nodeReloadPackagesAction(node) {
  return {
    type: NODE_RELOAD_PACKAGES
  };
}
